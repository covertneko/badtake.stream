using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BadTakeStream.Shared;
using BadTakeStream.Shared.Entities;
using BadTakeStream.Shared.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using StackExchange.Redis;

namespace BadTakeStream.Feeder
{
    public class FeederService : IHostedService, IDisposable
    {
        private Settings _settings;
        private IServiceProvider _serviceProvider;
        private Timer _streamCheckTimer;
        private ConnectionMultiplexer _redis;
        private Tweetinvi.Streaming.IFilteredStream _stream;

        private Timer _scoreUpdateTimer;
        private List<HighScore> _currentScores;
        private Metrics _currentMetrics;

        public FeederService(Settings settings, IServiceProvider serviceProvider)
        {
            _settings = settings;
            _serviceProvider = serviceProvider;
        }

        public static string Base64Encode(string plainText)
        {
            if (plainText == null)
                return null;

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var redisOptions = new ConfigurationOptions
            {
                EndPoints = { _settings.RedisHost },
                AbortOnConnectFail = false
            };

            _redis = await ConnectionMultiplexer.ConnectAsync(redisOptions);

            // Check if twitter credentials are actually missing because sometimes missing/bad credential
            // errors from tweetinvi and the twitter API are misleading, especially if docker is being buggy
            var missingSettings = new[]
            {
                _settings.TwitterConsumerKey,
                _settings.TwitterConsumerSecret,
                _settings.TwitterAccessToken,
                _settings.TwitterAccessSecret
            }.Any(string.IsNullOrEmpty);

            if (missingSettings)
                throw new ApplicationException("Twitter credentials are missing or empty");

            // Streaming endpoints require user credentials, rather than application-only
            Tweetinvi.Auth.SetUserCredentials(
                _settings.TwitterConsumerKey,
                _settings.TwitterConsumerSecret,
                _settings.TwitterAccessToken,
                _settings.TwitterAccessSecret
            );
            Tweetinvi.RateLimit.RateLimitTrackerMode = Tweetinvi.RateLimitTrackerMode.TrackAndAwait;

            _stream = Tweetinvi.Stream.CreateFilteredStream();
            _stream.AddTrack(_settings.TwitterFilterPhrase);

            _stream.MatchingTweetReceived += async (s, args) => await OnMatchingTweetReceived(s, args);
            _stream.StreamStopped += (s, args) =>
                Log.Error(args.Exception, "Stream stopped: {DisconnectMessage}", args.DisconnectMessage);

            // Ensure stream is running every 30 seconds
            _streamCheckTimer = new Timer(EnsureActive, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

            // Calculate scores every 10 seconds
            _scoreUpdateTimer = new Timer(CalculateScores, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        protected async Task OnMatchingTweetReceived(object sender, Tweetinvi.Events.MatchedTweetReceivedEventArgs args)
        {
            try
            {
                // Only include replies to public accounts
                // TODO: consider being a bit smarter with filtering beyond just matching a substring?
                if (args.Tweet.CreatedBy != null && args.Tweet.InReplyToStatusId.HasValue)
                {
                    var target = Tweetinvi.Tweet.GetTweet(args.Tweet.InReplyToStatusId.Value);

                    if (target?.CreatedBy == null)
                        return;

                    await ProcessMatch(args.Tweet, target);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to process tweet {Id}", args.Tweet.Id);
            }
        }

        protected void EnsureActive(object state)
        {
            if (_stream.StreamState != Tweetinvi.Models.StreamState.Running)
            {
                Log.Information("Starting stream");
                _ = _stream.StartStreamMatchingAllConditionsAsync();
            }
        }

        protected void CalculateScores(object state)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BadTakeContext>();

            // Include top 5 scores, along with top tweet for each user
            // TODO: review performance - this was bringing a t3.micro database server to its knees when running
            //       every 2-3 seconds on a per-tweet basis
            // TODO: revisit this - due to limitations in EF core when it comes to nested grouping and ordering
            //       via LINQ to entities, raw sql was an easier option
            _currentScores = db.HighScores.FromSqlRaw(@"
                SELECT
                    s.""Count"",
                    s.""UserId"",
                    s.""UserDisplayName""
                FROM ""Scores"" s
                ORDER BY s.""Count"" DESC
                LIMIT 5;
            ").ToList();

            // Calculate incoming tweet rate based on the last minute
            var recently = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1));
            var recentCount = db.Tweets.Where(t => t.InReplyTo != null && t.CreatedAt > recently).Count();
            var rate = decimal.Round(recentCount / 60.0m, 2, MidpointRounding.AwayFromZero);

            var today = DateTime.UtcNow.Date;

            _currentMetrics = new Metrics
            {
                Rate = rate,
                TotalToday = db.Tweets.Where(t => t.InReplyTo != null && t.CreatedAt > today).Count(),
                Total = db.Tweets.Where(t => t.InReplyTo != null).Count(),
                HighScores = _currentScores ?? new List<HighScore>()
            };
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _scoreUpdateTimer?.Change(Timeout.Infinite, 0);

            Log.Information("Stopping stream");

            _streamCheckTimer?.Change(Timeout.Infinite, 0);

            if (_stream != null && _stream.StreamState != Tweetinvi.Models.StreamState.Stop)
                _stream.StopStream();

            return Task.CompletedTask;
        }

        async Task ProcessMatch(Tweetinvi.Models.ITweet source, Tweetinvi.Models.ITweet target)
        {
            Log.Verbose("Matched tweet {TweetId}", source.Id);

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BadTakeContext>();

            var sourceEntity = new Tweet
            {
                TwitterId = source.Id,
                UserId = source.CreatedBy.Id,
                CreatedAt = source.CreatedAt.ToUniversalTime()
            };
            db.Add(sourceEntity);

            var targetEntity = db.Tweets.FirstOrDefault(t => t.TwitterId == target.Id);
            if (targetEntity == null)
            {
                targetEntity = new Tweet
                {
                    TwitterId = target.Id,
                    UserId = target.CreatedBy.Id,
                    CreatedAt = target.CreatedAt.ToUniversalTime()
                };

                db.Add(targetEntity);
            }

            sourceEntity.InReplyTo = targetEntity;

            var score = db.Scores.FirstOrDefault(s => s.UserId == targetEntity.UserId);
            if (score == null)
            {
                score = new Score
                {
                    UserId = targetEntity.UserId,
                    UserDisplayName = target.CreatedBy.Name
                };

                db.Add(score);
            }

            score.Count++;

            db.SaveChanges();

            // Construct and send update to clients
            var update = new FeedUpdate
            {
                Match = new Match
                {
                    Source = new TweetModel(source),
                    Target = new TweetModel(target)
                },
                Metrics = _currentMetrics
            };

            // Notify clients
            await _redis.GetSubscriber().PublishAsync(_settings.RedisChannel, JsonConvert.SerializeObject(update));
        }

        public void Dispose()
        {
            _streamCheckTimer?.Dispose();
        }
    }
}

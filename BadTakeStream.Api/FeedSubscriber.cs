using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BadTakeStream.Hubs;
using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR;
using BadTakeStream.Shared.Models;
using System.Threading;
using BadTakeStream.Shared;
using Serilog;
using System.Collections.Concurrent;
using StackExchange.Redis;

namespace BadTakeStream.Api
{
    public class FeedState
    {
        public Metrics Metrics { get; set; } = new Metrics();
        public ConcurrentQueue<Match> RecentMatches { get; set; } = new ConcurrentQueue<Match>();
    }

    public class FeedSubscriber : IHostedService
    {
        private readonly IHubContext<FeedHub, IFeedHubClient> _hubContext;
        private readonly Settings _appSettings;

        private ConnectionMultiplexer Redis { get; set; }

        public FeedState State { get; internal set; } = new FeedState();

        public FeedSubscriber(IHubContext<FeedHub, IFeedHubClient> hubContext, Settings appSettings)
        {
            _hubContext = hubContext;
            _appSettings = appSettings;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Redis = await ConnectionMultiplexer.ConnectAsync(_appSettings.RedisHost);

            await Redis.GetSubscriber().SubscribeAsync(
                _appSettings.RedisChannel,
                async (channel, message) =>
                {
                    var json = (string)message;
                    var update = JsonConvert.DeserializeObject<FeedUpdate>(json);

                    Log.Verbose("Got update for tweet {TweetId}", update.Match.Source.Id);

                    State.Metrics = update.Metrics;

                    // Keep 8 most recent matches for new page loads
                    State.RecentMatches.Enqueue(update.Match);
                    while(State.RecentMatches.Count > 8 && State.RecentMatches.TryDequeue(out _));

                    await _hubContext.Clients.All.AddMatch(update.Match);
                    await _hubContext.Clients.All.UpdateMetrics(update.Metrics);
                }
            );
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

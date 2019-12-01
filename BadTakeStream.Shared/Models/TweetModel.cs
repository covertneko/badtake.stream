using System;
using System.Collections.Generic;
using System.Web;

namespace BadTakeStream.Shared.Models
{
    public class TweetModel
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }

        public string AvatarUrl { get; set; }
        public string ProfileUrl { get; set; }
        public string Url { get; set; }

        public string Text { get; set; }

        public DateTime CreatedAt { get; set; }

        public TweetModel() { }

        public TweetModel(Tweetinvi.Models.ITweet source)
        {
            Id = source.Id;

            UserId = source.CreatedBy.Id;
            DisplayName = source.CreatedBy.Name;
            Username = HttpUtility.HtmlDecode(source.CreatedBy.ScreenName);

            AvatarUrl = source.CreatedBy.ProfileImageUrlHttps;

            ProfileUrl = $"https://twitter.com/{source.CreatedBy.ScreenName}";
            Url = source.Url;

            Text = HttpUtility.HtmlDecode(source.Text);
            CreatedAt = source.CreatedAt.ToUniversalTime();
        }
    }

    public class Match
    {
        public TweetModel Source { get; set; }

        public TweetModel Target { get; set; }
    }

    public class FeedUpdate
    {
        public Match Match { get; set; }
        public Metrics Metrics { get; set; }
    }

    public class HighScore
    {
        public long UserId { get; set; }
        public string UserDisplayName { get; set; }

        public long Count { get; set; }

        public string TopTweetUrl { get; set; }
        public long TopTweetCount { get; set; }
    }

    public class Metrics
    {
        public decimal Rate { get; set; }
        public int TotalToday { get; set; }
        public int Total { get; set; }

        public List<HighScore> HighScores { get; set; } = new List<HighScore>();
    }

}

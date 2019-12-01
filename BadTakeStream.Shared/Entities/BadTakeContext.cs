using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Npgsql.EntityFrameworkCore;

namespace BadTakeStream.Shared.Entities
{
    public class BadTakeContext : DbContext
    {
        public DbSet<Tweet> Tweets { get; set; }
        public DbSet<Score> Scores { get; set; }

        public DbSet<Models.HighScore> HighScores { get; set; }

        public BadTakeContext() { }

        public BadTakeContext(DbContextOptions<BadTakeContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Models.HighScore>(e => e.HasNoKey());
        }
    }

    public class Tweet
    {
        [Key]
        public long TwitterId { get; set; }

        public long UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        [InverseProperty(nameof(Replies))]
        public Tweet InReplyTo { get; set; }

        [InverseProperty(nameof(InReplyTo))]
        public List<Tweet> Replies { get; set; }
    }

    public class Score
    {
        [Key]
        public long UserId { get; set; }

        public string UserDisplayName { get; set; }

        public long Count { get; set; }
    }
}

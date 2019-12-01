using BadTakeStream.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BadTakeStream.Hubs
{
    public interface IFeedHubClient
    {
        Task AddMatch(Match match);

        Task UpdateMetrics(Metrics metrics);
    }

    public class FeedHub : Hub<IFeedHubClient>
    {
    }
}

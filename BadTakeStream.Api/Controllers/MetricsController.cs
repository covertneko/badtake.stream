using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BadTakeStream.Shared.Entities;
using BadTakeStream.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BadTakeStream.Api.Controllers
{
    [ApiController]
    [Route("metrics/[action]")]
    public class MetricsController : Controller
    {
        private readonly IServiceProvider _serviceProvider;

        public MetricsController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IActionResult Current()
        {
            using var scope = _serviceProvider.CreateScope();

            // TODO: find a way to always get FeedSubscriber rather than just any IHostedService.
            //       there's only one right now, so this works
            var feed = scope.ServiceProvider.GetRequiredService<IHostedService>() as FeedSubscriber;
            return Json(feed.State);
        }
    }
}
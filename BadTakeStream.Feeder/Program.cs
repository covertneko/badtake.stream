using BadTakeStream.Shared;
using BadTakeStream.Shared.Entities;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BadTakeStream.Feeder
{
    class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, args)
                => Log.Error(args.ExceptionObject as Exception, "Unhandled exception");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, false)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .CreateLogger();

                    logging
                        .ClearProviders()
                        .AddSerilog();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<MigratorService>();
                    services.AddHostedService<FeederService>();

                    services.AddSettings<Settings>(nameof(BadTakeStream));

                    services.AddApplicationInsightsTelemetryWorkerService();

                    var provider = services.BuildServiceProvider();

                    // Manually add dependency tracking since it's not enabled by default for console apps
                    // TODO: review placement of this
                    var aiConfig = provider.GetRequiredService<TelemetryConfiguration>();
                    var depModule = new DependencyTrackingTelemetryModule();
                    depModule.Initialize(aiConfig);

                    var settings = provider.GetRequiredService<Settings>();
                    services.AddDbContext<BadTakeContext>(options => options.UseNpgsql(
                        settings.DatabaseConnectionString,
                        p => p.MigrationsAssembly($"{nameof(BadTakeStream)}.Shared")
                    ));
                })
                .UseConsoleLifetime();
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BadTakeStream.Shared
{
    public interface IAppSettings { }

    public class Settings : IAppSettings
    {
        public string TwitterConsumerKey { get; set; }
        public string TwitterConsumerSecret { get; set; }
        public string TwitterAccessToken { get; set; }
        public string TwitterAccessSecret { get; set; }
        public string TwitterFilterPhrase { get; set; }

        public string RedisChannel { get; set; }
        public string RedisHost { get; set; }

        public string DatabaseConnectionString { get; set; }
    }

    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Use strongly-typed settings for configuration
        /// </summary>
        public static IServiceCollection AddSettings<T>(
            this IServiceCollection services,
            string prefix = null
        ) where T : class, IAppSettings, new()
        {
            var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

            if (!string.IsNullOrEmpty(prefix))
                config = config.GetSection(prefix);

            var settings = new T();
            config.Bind(settings);

            return services.AddSingleton(settings);
        }
    }
}

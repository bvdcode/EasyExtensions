using System;
using Sentry.AspNetCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using EasyExtensions.AspNetCore.Sentry.Factories;
using Sentry;

namespace EasyExtensions.AspNetCore.Sentry.Extensions
{
    /// <summary>
    /// <see cref="IWebHostBuilder"/> extensions.
    /// </summary>
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// Use Sentry integration with specified DSN.
        /// </summary>
        /// <param name="builder"> Current <see cref="IWebHostBuilder"/> instance. </param>
        /// <param name="dsn"> Sentry DSN. </param>
        /// <param name="setup"> Optional setup action. </param>
        /// <param name="useInDevelopment"> Force use in development environment. </param>
        /// <returns> Current <see cref="IWebHostBuilder"/> instance. </returns>
        public static IWebHostBuilder UseSentryWithUserCapturing(this IWebHostBuilder builder, 
            string dsn, Action<SentryAspNetCoreOptions>? setup = null, bool useInDevelopment = false)
        {
            bool isDevelopment = Environment.GetEnvironmentVariable("ENVIRONMENT") == "Development" || Debugger.IsAttached;
            if (isDevelopment && !useInDevelopment)
            {
                return builder;
            }
            if (string.IsNullOrWhiteSpace(dsn))
            {
                throw new ArgumentNullException(nameof(dsn));
            }
            var descriptor = new ServiceDescriptor(typeof(ISentryUserFactory), typeof(UserFactory), ServiceLifetime.Scoped);
            builder.ConfigureServices(x => x.Add(descriptor));
            return builder.UseSentry(x =>
            {
                x.Dsn = dsn;
                x.SendDefaultPii = true;
                setup?.Invoke(x);
            });
        }
    }
}
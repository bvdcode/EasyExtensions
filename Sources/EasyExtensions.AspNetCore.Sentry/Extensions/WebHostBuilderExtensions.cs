using System;
using Sentry.AspNetCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using EasyExtensions.Sentry.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace EasyExtensions.Sentry.Extensions
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
        /// <param name="forceUseInDevelopment"> Force use in development environment. </param>
        /// <returns> Current <see cref="IWebHostBuilder"/> instance. </returns>
        public static IWebHostBuilder UseSentryWithUserCapturing(this IWebHostBuilder builder, string dsn, bool forceUseInDevelopment = false)
        {
            bool isDevelopment = Environment.GetEnvironmentVariable("ENVIRONMENT") == "Development" || Debugger.IsAttached;
            if (isDevelopment && !forceUseInDevelopment)
            {
                return builder;
            }
            if (string.IsNullOrWhiteSpace(dsn))
            {
                throw new ArgumentNullException(nameof(dsn));
            }
            var descriptor = new ServiceDescriptor(typeof(IUserFactory), typeof(UserFactory), ServiceLifetime.Scoped);
            builder.ConfigureServices(x => x.Add(descriptor));
            return builder.UseSentry(x =>
            {
                x.Dsn = dsn;
                x.SendDefaultPii = true;
            });
        }
    }
}
using EasyExtensions.AspNetCore.Authorization.Extensions;
using EasyExtensions.AspNetCore.Extensions;
using EasyExtensions.AspNetCore.Stack.Builders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace EasyExtensions.AspNetCore.Stack.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring EasyStack services and features on an application builder.
    /// </summary>
    /// <remarks>This static class contains extension methods that enable the registration and configuration
    /// of EasyStack components, such as security, logging, and optional integrations, during application startup. Use
    /// these methods to ensure all required EasyStack services are available for dependency injection and properly
    /// configured according to application needs.</remarks>
    public static class HostApplicationBuilderExtensions
    {
        /// <summary>
        /// Configures the EasyStack suite of services and features for the application, including security, logging,
        /// and optional integrations, using the specified options.
        /// <code>
        /// Already added:
        /// * Controllers
        /// * EasyLogging Formatters
        /// * Quartz Jobs
        /// * SignalR
        /// * CORS Origins - if presented in configuration "CorsOrigins", or allow all if not
        /// * Healthchecks
        /// * Pbkdf2PasswordHashService - if "Pepper" presented in configuration, 16+ length
        ///
        /// Optional:
        /// * Postgres - options.AddPostgres
        /// * Auth (requires Postgres, Pbkdf2PasswordHashService) - options.AddAuthorization
        /// * EasyVault - if VaultApiUrl presented in configuration
        /// </code>
        /// </summary>
        /// <remarks>This method adds and configures core EasyStack services such as password hashing,
        /// secret management, and other optional integrations based on the application's configuration and the provided
        /// options. Some features are enabled only if corresponding configuration values are present. Call this method
        /// early in the application's startup to ensure all EasyStack services are available for dependency
        /// injection.</remarks>
        /// <param name="builder">The application builder to configure. Must not be null.</param>
        /// <param name="setupStack">An optional delegate to configure EasyStack options. If null, default options are used.</param>
        /// <returns>The same <see cref="IHostApplicationBuilder"/> instance so that additional configuration calls can be
        /// chained.</returns>
        public static IHostApplicationBuilder AddEasyStack(
            this IHostApplicationBuilder builder,
            Action<EasyStackOptions>? setupStack = null)
        {
            // create temp logger to log during startup
            using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder.AddSimpleConsoleLogging();
            });
            var logger = loggerFactory.CreateLogger("EasyStack");
            logger.LogInformation("Starting EasyStack setup...");

            EasyStackOptions options = new();
            setupStack?.Invoke(options);
            logger.LogInformation("Configuration of EasyStack options is {state}.", setupStack is null ? "null, using defaults" : "provided");

            // EasyLogging Formatters
            builder.Logging.AddSimpleConsoleLogging();
            builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            builder.Logging.AddFilter("LuckyPennySoftware.AutoMapper.License", LogLevel.None);
            logger.LogInformation("Added EasyLogging formatters");

            // Healthchecks
            var healthChecksBuilder = builder.Services.AddHealthChecks();
            logger.LogInformation("Added health checks");

            // Controllers
            builder.Services.AddControllers();
            logger.LogInformation("Added controllers");

            // Compression
            builder.Services.AddResponseCompression(x =>
            {
                x.EnableForHttps = true;
                x.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes;
            });
            logger.LogInformation("Added response compression for {count} mime types",
                Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Count());
            foreach (var mime in Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes)
            {
                logger.LogInformation(" - Compression MimeType: {mime}", mime);
            }

            // Quartz Jobs
            logger.LogInformation("Adding Quartz jobs...");
            Quartz.Extensions.ServiceCollectionExtensions.AddQuartzJobs(builder.Services, x =>
            {
                logger.LogInformation("Found job added: {job}", x.Name);
            });

            // SignalR
            builder.Services.AddSignalR();
            logger.LogInformation("Added SignalR");

            // Setup Origins if presented in configuration, or allow all if asked
            string[] corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? [];
            if (corsOrigins.Length > 0)
            {
                builder.Services.AddDefaultCorsWithOrigins(corsOrigins);
                logger.LogInformation("Added CORS with {count} origins from configuration", corsOrigins.Length);
                foreach (var origin in corsOrigins)
                {
                    logger.LogInformation(" - CORS Origin: {origin}", origin);
                }
            }
            else
            {
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .WithExposedHeaders("*");
                    });
                });
                logger.LogWarning("No CORS origins found in configuration - allowing ALL origins.");
            }

            // EasyVault - if URL presented in configuration
            bool isVaultPresented = !string.IsNullOrWhiteSpace(builder.Configuration[EasyVault.SDK.Extensions.ConfigurationExtensions.DefaultServerUrlKey]);
            if (isVaultPresented)
            {
                EasyVault.SDK.Extensions.ConfigurationExtensions.AddSecrets(builder.Configuration);
                logger.LogInformation("VaultApiUrl found in configuration, added EasyVault");
            }

            // Add Pbkdf2
            bool isPepperPresented = !string.IsNullOrWhiteSpace(builder.Configuration[AspNetCore.Extensions.ServiceCollectionExtensions.Pbkdf2PepperDefaultConfigurationKey]);
            if (isPepperPresented)
            {
                builder.Services.AddPbkdf2PasswordHashService();
                logger.LogInformation("Pepper found in configuration, added Pbkdf2PasswordHashService");
            }

            // Add Postgres if asked
            if (options.ConfigureDatabase != null)
            {
                options.ConfigureDatabase.Invoke(builder.Services, healthChecksBuilder, builder.Configuration);
                logger.LogInformation("Added Postgres DbContext and Database health check");
            }

            if (options.AuthorizationEnabled)
            {
                if (!isPepperPresented)
                {
                    throw new Exception("Authorization requires Pbkdf2PasswordHashService. Set configuration key 'EasyExtensions:Services:Pbkdf2:Pepper'.");
                }

                // add jwt
                builder.Services.AddJwt();
                logger.LogInformation("Added JWT authentication");
            }

            return builder;
        }
    }
}

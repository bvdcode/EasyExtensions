using EasyExtensions.Abstractions;
using EasyExtensions.AspNetCore.Formatters;
using EasyExtensions.AspNetCore.HealthChecks;
using EasyExtensions.Helpers;
using EasyExtensions.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EasyExtensions.AspNetCore.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Represents the configuration key used to specify the pepper value for PBKDF2 operations.
        /// </summary>
        /// <remarks>Use this key when retrieving or setting the pepper value in configuration sources
        /// related to PBKDF2 password hashing. The pepper is an additional secret value that enhances password security
        /// when combined with a salt.</remarks>
        public const string Pbkdf2PepperDefaultConfigurationKey = "Pepper";

        /// <summary>
        /// Adds <see cref="SimpleConsoleFormatter"/> to the <see cref="ILoggingBuilder"/>.
        /// </summary>
        /// <param name="builder"> Current <see cref="ILoggingBuilder"/> instance. </param>
        public static ILoggingBuilder AddSimpleConsoleLogging(this ILoggingBuilder builder)
        {
            return builder.AddConsole(o => o.FormatterName = SimpleConsoleFormatter.FormatterName)
                .AddConsoleFormatter<SimpleConsoleFormatter, SimpleConsoleFormatterOptions>();
        }

        /// <summary>
        /// Adds <see cref="Pbkdf2PasswordHashService"/> to the <see cref="IServiceCollection"/> resolving pepper from <see cref="IConfiguration"/> in DI.
        /// </summary>
        /// <param name="services"> Current <see cref="IServiceCollection"/> instance. </param>
        /// <param name="configurationKey"> Configuration key to resolve pepper from <see cref="IConfiguration"/>. Default is <see cref="Pbkdf2PepperDefaultConfigurationKey"/>. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when Pepper configuration value is missing or invalid. </exception>
        /// <remarks>
        /// Requires IConfiguration in the service provider with provided pepper (at least 16 UTF-8 bytes).
        /// Example: builder.Services.AddPbkdf2PasswordHashService();
        /// </remarks>
        public static IServiceCollection AddPbkdf2PasswordHashService(this IServiceCollection services, string configurationKey = Pbkdf2PepperDefaultConfigurationKey)
        {
            return services.AddSingleton<IPasswordHashService>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var pepper = configuration[configurationKey]
                    ?? throw new ArgumentNullException(nameof(configurationKey), "Pepper configuration value is missing: " + configurationKey);
                return new Pbkdf2PasswordHashService(pepper);
            });
        }

        /// <summary>
        /// Adds default health checks to the <see cref="IServiceCollection"/> instance.
        /// </summary>
        /// <param name="services"> <see cref="IServiceCollection"/> instance. </param>
        /// <param name="setup"> Action to setup health checks. </param>
        /// <param name="configure"> Action to configure health check options. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddDefaultHealthChecks(this IServiceCollection services, Action<IHealthChecksBuilder>? setup = null, Action<HealthCheckOptions>? configure = null)
        {
            var builder = services.AddHealthChecks();
            HealthCheckOptions options = new();
            configure?.Invoke(options);
            if (options.Dns)
            {
                builder.AddCheck<DnsHealthCheck>("DNS", tags: ["network"]);
            }
            if (options.Internet)
            {
                builder.AddCheck<InternetHealthCheck>("Internet", tags: ["network"]);
            }
            if (options.Network)
            {
                builder.AddCheck<NetworkHealthCheck>("Network", tags: ["network"]);
            }
            if (options.DiskSpace)
            {
                builder.AddCheck<DiskSpaceHealthCheck>("Disk Space", tags: ["disk"]);
            }
            if (options.Memory)
            {
                builder.AddCheck<MemoryHealthCheck>("Memory", tags: ["memory"]);
            }
            setup?.Invoke(builder);
            return services;
        }

        /// <summary>
        /// Adds CORS policy with origins.
        /// </summary>
        /// <param name="services"> <see cref="IServiceCollection"/> instance. </param>
        /// <param name="policyName"> Name of the policy. </param>
        /// <param name="origins"> Origins to add to the policy. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddCorsWithOrigins(this IServiceCollection services, string policyName, params string[] origins)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: policyName,
                    policy =>
                    {
                        policy.WithOrigins(origins)
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .AllowAnyMethod()
                            .WithExposedHeaders("*");
                    });
            });
            return services;
        }

        /// <summary>
        /// Adds default CORS policy with origins.
        /// </summary>
        /// <param name="services"> <see cref="IServiceCollection"/> instance. </param>
        /// <param name="origins"> Origins to add to the policy. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddDefaultCorsWithOrigins(this IServiceCollection services, params string[] origins)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .WithExposedHeaders("*");
                });
            });
            return services;
        }

        /// <summary>
        /// Adds exception handler for EasyExtensions.*.Exceptions to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"> The <see cref="IServiceCollection"/> instance. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddExceptionHandler(this IServiceCollection services)
        {
            services.AddExceptionHandler(o => o.ExceptionHandler = HandleException);
            return services;
        }

        /// <summary>
        /// Adds <see cref="CpuUsageService"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"> Current <see cref="IServiceCollection"/> instance. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddCpuUsageService(this IServiceCollection services)
        {
            services.AddSingleton(new CpuUsageService());
            return services;
        }

        /// <summary>
        /// Add all types inherited from TInterface.
        /// </summary>
        /// <param name="services"> Current <see cref="IServiceCollection"/> instance. </param>
        /// <param name="serviceLifetime"> Service lifetime, default is Scoped. </param>
        /// <typeparam name="TInterface"> Interface type. </typeparam>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddTypesOfInterface<TInterface>(this IServiceCollection services,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where TInterface : class
        {
            var types = ReflectionHelpers.GetTypesOfInterface<TInterface>();
            if (!types.Any())
            {
                throw new TypeLoadException($"No types inherited from {typeof(TInterface).Name} found.");
            }
            foreach (var type in types)
            {
                var typeInterfaces = type.GetInterfaces();
                var genericType = typeInterfaces.FirstOrDefault(i => i.IsGenericType && i.GetInterfaces().Any(x => x == typeof(TInterface)));
                var contract = genericType ?? typeof(TInterface);
                var descriptor = new ServiceDescriptor(contract, type, serviceLifetime);
                services.Add(descriptor);
            }
            return services;
        }

        private static async Task HandleException(HttpContext context)
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>()!;
            var exception = exceptionHandlerPathFeature.Error;
            if (exception is IHttpError httpError)
            {
                context.Response.StatusCode = (int)httpError.StatusCode;
                await context.Response.WriteAsJsonAsync(httpError.GetErrorModel());
            }
        }
    }
}
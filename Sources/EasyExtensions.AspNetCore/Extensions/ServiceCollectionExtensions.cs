using System;
using System.Linq;
using System.Threading.Tasks;
using EasyExtensions.Helpers;
using EasyExtensions.Services;
using Microsoft.AspNetCore.Http;
using EasyExtensions.Abstractions;
using Microsoft.AspNetCore.Diagnostics;
using EasyExtensions.AspNetCore.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace EasyExtensions.AspNetCore.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
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
        /// Adds exception handler for EasyExtensions.EntityFrameworkCore.Exceptions to the <see cref="IServiceCollection"/>.
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
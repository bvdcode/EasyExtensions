﻿using Npgsql;
using System;
using Gridify;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EasyExtensions.EntityFrameworkCore.Exceptions;

namespace EasyExtensions.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
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
        /// Sets up Gridify.
        /// </summary>
        public static IServiceCollection AddGridifyMappers(this IServiceCollection services)
        {
            GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
            GridifyGlobalConfiguration.DefaultPageSize = 20;
            services.AddGridifyMappers(Assembly.GetCallingAssembly());
            return services;
        }

        /// <summary>
        /// Adds a <see cref="DbContext"/> to the <see cref="IServiceCollection"/> using the 
        /// <see cref="IConfigurationRoot"/> to build the connection string from DatabaseSettings section.
        /// </summary>
        /// <typeparam name="TContext"> The type of <see cref="DbContext"/> to add. </typeparam>
        /// <param name="services"> The <see cref="IServiceCollection"/> instance. </param>
        /// <param name="configuration"> The <see cref="IConfigurationRoot"/> instance. </param>
        /// <param name="maxPoolSize"> The maximum pool size, default is 100. </param>
        /// <param name="timeout_s"> The connection timeout in seconds, default is 60. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        /// <exception cref="KeyNotFoundException"> When DatabaseSettings section is not set. </exception>
        public static IServiceCollection AddDbContext<TContext>(this IServiceCollection services, IConfigurationRoot configuration, int maxPoolSize = 100, int timeout_s = 60)
            where TContext : DbContext
        {
            string connectionString = BuildConnectionString(configuration, maxPoolSize, timeout_s);
            return services.AddDbContext<TContext>(builder =>
            {
                builder
                    .UseNpgsql(connectionString)
                    .UseLazyLoadingProxies();
            });
        }

        private static string BuildConnectionString(IConfigurationRoot configuration, int maxPoolSize, int timeout_s = 60)
        {
            bool isDevelopment = (Environment.GetEnvironmentVariable("ENVIRONMENT") ?? string.Empty) == "Development";
            if (!isDevelopment)
            {
                isDevelopment = (configuration["ASPNETCORE_ENVIRONMENT"] ?? string.Empty) == "Development";
            }
            var settings = configuration.GetSection("DatabaseSettings");
            string server = (!settings.Exists() ? configuration["DatabaseServer"] : settings["Server"]) ?? throw new KeyNotFoundException("DatabaseSettings.Server or DatabaseServer is not set");
            int port = (!settings.Exists() ? configuration.GetValue<ushort>("DatabasePort") : settings.GetValue<ushort>("Port"));
            if (port == default)
            {
                throw new KeyNotFoundException("DatabaseSettings.Port or DatabasePort is not set");
            }
            string username = (!settings.Exists() ? configuration["DatabaseUsername"] : settings["Username"])
                ?? throw new KeyNotFoundException("DatabaseSettings.Username is not set");

            string password = (!settings.Exists() ? configuration["DatabasePassword"] : settings["Password"])
                ?? throw new KeyNotFoundException("DatabaseSettings.Password is not set");

            string database = (!settings.Exists() ? configuration["DatabaseName"] : settings["Database"])
                ?? throw new KeyNotFoundException("DatabaseSettings.Database is not set");
            if (isDevelopment)
            {
                database = (!settings.Exists() ? configuration["DatabaseNameDev"] : settings["DatabaseDev"]) ?? database;
            }
            NpgsqlConnectionStringBuilder builder = new()
            {
                Username = username,
                Password = password,
                Host = server,
                Database = database,
                Port = port,
                IncludeErrorDetail = true,
                Timezone = "UTC",
                MaxPoolSize = maxPoolSize,
                Timeout = timeout_s,
                CommandTimeout = timeout_s
            };
            return builder.ConnectionString;
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
﻿using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using EasyExtensions.EntityFrameworkCore.Npgsql.Factories;
using EasyExtensions.EntityFrameworkCore.Npgsql.Migrations;

namespace EasyExtensions.EntityFrameworkCore.Npgsql.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a <see cref="DbContext"/> to the <see cref="IServiceCollection"/> using the 
        /// <see cref="IConfiguration"/> to build the connection string from DatabaseSettings section.
        /// </summary>
        /// <typeparam name="TContext"> The type of <see cref="DbContext"/> to add. </typeparam>
        /// <param name="services"> The <see cref="IServiceCollection"/> instance. </param>
        /// <param name="configuration"> The <see cref="IConfiguration"/> instance. </param>
        /// <param name="setup"> The action to setup the <see cref="PostgresContextFactory"/>. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        /// <exception cref="KeyNotFoundException"> When DatabaseSettings section is not set. </exception>
        public static IServiceCollection AddPostgresDbContext<TContext>(this IServiceCollection services,
            IConfiguration configuration, Action<PostgresContextFactory>? setup = null) where TContext : DbContext
        {
            PostgresContextFactory contextFactory = new();
            setup?.Invoke(contextFactory);
            string connectionString = BuildConnectionString(configuration, contextFactory);
            services.AddDbContext<TContext>(builder =>
            {
                builder
                    .UseNpgsql(connectionString)
                    .UseLazyLoadingProxies();
            }, contextLifetime: contextFactory.ContextLifetime);
            if (contextFactory.AddDesignTimeDbContextFactory)
            {
                services.AddScoped<IDesignTimeDbContextFactory<TContext>, DesignTimeDbContextFactory<TContext>>();
            }
            return services;
        }

        private static string BuildConnectionString(IConfiguration configuration, PostgresContextFactory contextFactory)
        {
            bool isDevelopment = GetIsDevelopment(configuration);
            var settings = configuration.GetSection(contextFactory.ConfigurationSection);
            string host = GetSetting(settings, "Host", configuration, contextFactory);
            string portStr = GetSetting(settings, "Port", configuration, contextFactory);
            string username = GetSetting(settings, "Username", configuration, contextFactory);
            string password = GetSetting(settings, "Password", configuration, contextFactory);
            string database = GetSetting(settings, "Database", configuration, contextFactory);
            if (isDevelopment)
            {
                database = TryGetSetting(settings, "DatabaseDev", configuration, contextFactory) ?? database;
            }
            NpgsqlConnectionStringBuilder builder = new()
            {
                Host = host,
                Username = username,
                Password = password,
                Database = database,
                IncludeErrorDetail = true,
                Port = ushort.Parse(portStr),
                Timezone = contextFactory.Timezone,
                Encoding = contextFactory.Encoding,
                Timeout = contextFactory.TimeoutSeconds,
                MaxPoolSize = contextFactory.MaxPoolSize,
                CommandTimeout = contextFactory.TimeoutSeconds,
            };
            contextFactory.SetupConnectionString?.Invoke(builder);
            return builder.ConnectionString;
        }

        private static string? TryGetSetting(IConfigurationSection settings, string key, IConfiguration configuration, PostgresContextFactory contextFactory)
        {
            if (configuration[contextFactory.ConfigurationPrefix + key] is string value)
            {
                return value;
            }
            if (!settings.Exists())
            {
                return null;
            }
            return settings[key];
        }

        private static string GetSetting(IConfigurationSection settings, string key, IConfiguration configuration, PostgresContextFactory contextFactory)
        {
            if (configuration[contextFactory.ConfigurationPrefix + key] is string value)
            {
                return value;
            }
            if (!settings.Exists())
            {
                throw new KeyNotFoundException($"{contextFactory.ConfigurationSection} section or {contextFactory.ConfigurationPrefix}{key} is not set");
            }
            return settings[key] ?? throw new KeyNotFoundException($"{settings.Path}:{key} is not set");
        }

        private static bool GetIsDevelopment(IConfiguration configuration)
        {
            bool result = (Environment.GetEnvironmentVariable("ENVIRONMENT") ?? string.Empty) == "Development";
            if (!result)
            {
                result = (configuration["ASPNETCORE_ENVIRONMENT"] ?? string.Empty) == "Development";
            }
            return result;
        }
    }
}
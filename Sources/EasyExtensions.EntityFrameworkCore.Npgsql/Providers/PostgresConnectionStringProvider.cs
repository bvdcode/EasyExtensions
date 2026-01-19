// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Abstractions;
using EasyExtensions.EntityFrameworkCore.Npgsql.Builders;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EasyExtensions.EntityFrameworkCore.Npgsql.Providers
{
    /// <summary>
    /// Provides a mechanism for retrieving PostgreSQL database connection strings based on application configuration
    /// and specified options.
    /// </summary>
    /// <remarks>This provider combines values from the supplied configuration and options to build a
    /// connection string suitable for use with Npgsql. It supports environment-specific overrides (such as development
    /// databases) and allows customization through the provided options. The resulting connection string reflects the
    /// current application environment and configuration state.</remarks>
    /// <param name="cfg">The configuration source used to retrieve connection string settings and environment information.</param>
    /// <param name="options">The options that define how the PostgreSQL connection string is constructed, including section names, prefixes,
    /// and additional connection parameters.</param>
    public class PostgresConnectionStringProvider(IConfiguration cfg,
        PostgresOptionsBuilder options) : IPostgresConnectionStringProvider
    {
        /// <summary>
        /// Retrieves the database connection string based on the current configuration and options.
        /// </summary>
        /// <returns>A string containing the database connection string. The format and contents depend on the current
        /// configuration settings.</returns>
        public string GetConnectionString()
        {
            return BuildConnectionString(cfg, options);
        }

        private static string BuildConnectionString(IConfiguration configuration, PostgresOptionsBuilder contextFactory)
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
                Port = ushort.Parse(portStr),
                Timezone = contextFactory.Timezone,
                Encoding = contextFactory.Encoding,
                Timeout = contextFactory.TimeoutSeconds,
                MaxPoolSize = contextFactory.MaxPoolSize,
                CommandTimeout = contextFactory.TimeoutSeconds,
                IncludeErrorDetail = contextFactory.IncludeErrorDetail,
            };
            contextFactory.SetupConnectionString?.Invoke(builder);
            return builder.ConnectionString;
        }

        private static string? TryGetSetting(IConfigurationSection settings, string key, IConfiguration configuration, PostgresOptionsBuilder contextFactory)
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

        private static string GetSetting(IConfigurationSection settings,
            string key,
            IConfiguration configuration,
            PostgresOptionsBuilder contextFactory,
            string? defaultValue = null)
        {
            if (configuration[contextFactory.ConfigurationPrefix + key] is string value)
            {
                return value;
            }
            if (!settings.Exists())
            {
                if (defaultValue is not null)
                {
                    return defaultValue;
                }
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

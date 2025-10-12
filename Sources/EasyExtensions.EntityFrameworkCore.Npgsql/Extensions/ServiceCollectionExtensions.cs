using Npgsql;
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
        /// Adds a <see cref="DbContext"/> to the <see cref="IServiceCollection"/> resolving <see cref="IConfiguration"/> from DI.
        /// Builds the connection string from the configured section (default: "DatabaseSettings") and/or prefixed keys.
        /// </summary>
        /// <typeparam name="TContext"> The type of <see cref="DbContext"/> to add. </typeparam>
        /// <param name="services"> The <see cref="IServiceCollection"/> instance. </param>
        /// <param name="setup"> Optional action to configure the <see cref="PostgresOptionsBuilder"/> (section name, prefix, pool size, etc). </param>
        /// <param name="setupContextOptions"> Optional action to configure the <see cref="DbContextOptionsBuilder"/> (e.g. enable sensitive data logging). </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        /// <exception cref="KeyNotFoundException"> When required database settings are missing. </exception>
        /// <example>
        /// <code>
        /// builder.Services.AddPostgresDbContext&lt;MyDbContext&gt;(f =&gt; { f.ConfigurationSection = "Db"; });
        /// </code>
        /// </example>
        public static IServiceCollection AddPostgresDbContext<TContext>(this IServiceCollection services,
            Action<PostgresOptionsBuilder>? setup = null, Action<DbContextOptionsBuilder>? setupContextOptions = null) where TContext : DbContext
        {
            PostgresOptionsBuilder contextFactory = new();
            setup?.Invoke(contextFactory);

            services.AddDbContext<TContext>((sp, builder) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                string connectionString = BuildConnectionString(configuration, contextFactory);
                builder.UseNpgsql(connectionString);
                setupContextOptions?.Invoke(builder);
                if (contextFactory.UseLazyLoadingProxies)
                {
                    builder.UseLazyLoadingProxies();
                }
            }, contextLifetime: contextFactory.ContextLifetime);

            if (contextFactory.AddDesignTimeDbContextFactory)
            {
                services.AddScoped<IDesignTimeDbContextFactory<TContext>, DesignTimeDbContextFactory<TContext>>();
            }
            return services;
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

        private static string GetSetting(IConfigurationSection settings, string key, IConfiguration configuration, PostgresOptionsBuilder contextFactory)
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
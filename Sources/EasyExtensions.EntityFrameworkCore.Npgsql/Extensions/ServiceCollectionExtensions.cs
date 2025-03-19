using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using EasyExtensions.EntityFrameworkCore.Npgsql.Migrations;
using EasyExtensions.EntityFrameworkCore.Npgsql.Factories;

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
            IConfiguration configuration, Action<PostgresContextFactory>? setup) where TContext : DbContext
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
            bool isDevelopment = (Environment.GetEnvironmentVariable("ENVIRONMENT") ?? string.Empty) == "Development";
            if (!isDevelopment)
            {
                isDevelopment = (configuration["ASPNETCORE_ENVIRONMENT"] ?? string.Empty) == "Development";
            }
            var settings = configuration.GetSection(contextFactory.ConfigurationSection);
            string server = (!settings.Exists() ? configuration["DatabaseServer"] : settings["Server"]) ?? throw new KeyNotFoundException("DatabaseSettings.Server or DatabaseServer is not set");
            string databasePortStr = (!settings.Exists() ? configuration["DatabasePort"] : settings["Port"]) ?? throw new KeyNotFoundException("DatabaseSettings.Port or DatabasePort is not set");
            int port = int.TryParse(databasePortStr, out int parsedPort) ? parsedPort : throw new KeyNotFoundException("DatabaseSettings.Port or DatabasePort is not set");
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
                Timezone = contextFactory.Timezone,
                MaxPoolSize = contextFactory.MaxPoolSize,
                Timeout = contextFactory.TimeoutSeconds,
                CommandTimeout = contextFactory.TimeoutSeconds,
                Encoding = contextFactory.Encoding,
            };
            contextFactory.SetupConnectionString?.Invoke(builder);
            return builder.ConnectionString;
        }
    }
}
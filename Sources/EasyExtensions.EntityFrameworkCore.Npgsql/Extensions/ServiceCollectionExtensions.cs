using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using EasyExtensions.EntityFrameworkCore.Npgsql.Migrations;

namespace EasyExtensions.EntityFrameworkCore.Npgsql.Extensions
{
    /// <summary>
    /// This class is used to set up the <see cref="DbContext"/> for PostgreSQL with the specified arguments.
    /// </summary>
    public class PostgresContextFactory
    {
        /// <summary>
        /// The maximum pool size for the database connection, default is 100.
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// The connection timeout in seconds, default is 60 seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// The lifetime of the <see cref="DbContext"/>, default is <see cref="ServiceLifetime.Transient"/>.
        /// </summary>
        public ServiceLifetime ContextLifetime { get; set; } = ServiceLifetime.Transient;

        /// <summary>
        /// The timezone to use for the database connection, default is "UTC".
        /// </summary>
        public string Timezone { get; set; } = "UTC";

        /// <summary>
        /// The encoding to use for the database connection, default is "UTF8".
        /// </summary>
        public string Encoding { get; set; } = "UTF8";

        /// <summary>
        /// The section name in the <see cref="IConfiguration"/> to use for the database settings, default is "DatabaseSettings".
        /// </summary>
        public string ConfigurationSection { get; set; } = "DatabaseSettings";

        /// <summary>
        /// Whether to add the <see cref="IDesignTimeDbContextFactory{TContext}"/> to the service collection.
        /// </summary>
        public bool AddDesignTimeDbContextFactory { get; set; } = true;

        /// <summary>
        /// Setup action for the <see cref="NpgsqlConnectionStringBuilder"/> to customize the connection string.
        /// </summary>
        public Action<NpgsqlConnectionStringBuilder>? SetupConnectionString { get; set; }
    }

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


        /// <summary>
        /// Adds a <see cref="DbContext"/> to the <see cref="IServiceCollection"/> using the 
        /// <see cref="IConfiguration"/> to build the connection string from DatabaseSettings section.
        /// </summary>
        /// <typeparam name="TContext"> The type of <see cref="DbContext"/> to add. </typeparam>
        /// <param name="services"> The <see cref="IServiceCollection"/> instance. </param>
        /// <param name="configuration"> The <see cref="IConfiguration"/> instance. </param>
        /// <param name="maxPoolSize"> The maximum pool size, default is 100. </param>
        /// <param name="timeoutSeconds"> The connection timeout in seconds, default is 60. </param>
        /// <param name="contextLifetime"> The <see cref="ServiceLifetime"/> of the <see cref="DbContext"/>, default is Transient. </param>
        /// <param name="setupConnectionString"> The action to setup the connection string builder. </param>
        /// <param name="addDesignTimeDbContextFactory"> Whether to add the <see cref="IDesignTimeDbContextFactory{TContext}"/> to the service collection. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        /// <exception cref="KeyNotFoundException"> When DatabaseSettings section is not set. </exception>
        [Obsolete("Use AddPostgresDbContext<TContext>(IServiceCollection, IConfiguration, Action<PostgresContextFactory>) instead. This method will be removed in a future version.", false)]
        public static IServiceCollection AddPostgresDbContext<TContext>(this IServiceCollection services,
            IConfiguration configuration, int maxPoolSize = 100, int timeoutSeconds = 60, ServiceLifetime contextLifetime = ServiceLifetime.Transient,
            Action<NpgsqlConnectionStringBuilder>? setupConnectionString = null, bool addDesignTimeDbContextFactory = false)
            where TContext : DbContext
        {
            return AddPostgresDbContext<TContext>(services, configuration, x =>
            {
                x.MaxPoolSize = maxPoolSize;
                x.TimeoutSeconds = timeoutSeconds;
                x.ContextLifetime = contextLifetime;
                x.SetupConnectionString = setupConnectionString;
                x.AddDesignTimeDbContextFactory = addDesignTimeDbContextFactory;
            });
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
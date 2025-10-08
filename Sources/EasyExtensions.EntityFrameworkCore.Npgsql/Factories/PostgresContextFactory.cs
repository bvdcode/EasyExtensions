using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace EasyExtensions.EntityFrameworkCore.Npgsql.Factories
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
        /// The prefix to use for the configuration settings, default is "Postgres".
        /// </summary>
        public string ConfigurationPrefix { get; set; } = "Postgres";

        /// <summary>
        /// Whether to add the <see cref="IDesignTimeDbContextFactory{TContext}"/> to the service collection.
        /// </summary>
        public bool AddDesignTimeDbContextFactory { get; set; } = true;

        /// <summary>
        /// Setup action for the <see cref="NpgsqlConnectionStringBuilder"/> to customize the connection string.
        /// </summary>
        public Action<NpgsqlConnectionStringBuilder>? SetupConnectionString { get; set; }

        /// <summary>
        /// Whether to use lazy loading proxies, default is true.
        /// </summary>
        public bool UseLazyLoadingProxies { get; set; } = true;

        /// <summary>
        /// Whether to include error details in exceptions, default is true.
        /// </summary>
        public bool IncludeErrorDetail { get; set; } = true;
    }
}
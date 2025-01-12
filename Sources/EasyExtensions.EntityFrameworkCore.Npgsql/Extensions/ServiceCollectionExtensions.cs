using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        /// <param name="maxPoolSize"> The maximum pool size, default is 100. </param>
        /// <param name="timeoutSeconds"> The connection timeout in seconds, default is 60. </param>
        /// <param name="contextLifetime"> The <see cref="ServiceLifetime"/> of the <see cref="DbContext"/>, default is Transient. </param>
        /// <param name="setupConnectionString"> The action to setup the connection string builder. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        /// <exception cref="KeyNotFoundException"> When DatabaseSettings section is not set. </exception>
        public static IServiceCollection AddPostgresDbContext<TContext>(this IServiceCollection services,
            IConfiguration configuration, int maxPoolSize = 100, int timeoutSeconds = 60, 
            ServiceLifetime contextLifetime = ServiceLifetime.Transient, Action<NpgsqlConnectionStringBuilder>? setupConnectionString = null)
            where TContext : DbContext
        {
            string connectionString = BuildConnectionString(configuration, maxPoolSize, timeoutSeconds, setupConnectionString);
            return services.AddDbContext<TContext>(builder =>
            {
                builder
                    .UseNpgsql(connectionString)
                    .UseLazyLoadingProxies();
            }, contextLifetime: contextLifetime);
        }

        private static string BuildConnectionString(IConfiguration configuration, int maxPoolSize,
            int timeoutSeconds = 60, Action<NpgsqlConnectionStringBuilder>? setupConnectionString = null)
        {
            bool isDevelopment = (Environment.GetEnvironmentVariable("ENVIRONMENT") ?? string.Empty) == "Development";
            if (!isDevelopment)
            {
                isDevelopment = (configuration["ASPNETCORE_ENVIRONMENT"] ?? string.Empty) == "Development";
            }
            var settings = configuration.GetSection("DatabaseSettings");
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
                Timezone = "UTC",
                MaxPoolSize = maxPoolSize,
                Timeout = timeoutSeconds,
                CommandTimeout = timeoutSeconds,
                Encoding = "UTF8",
            };
            setupConnectionString?.Invoke(builder);
            return builder.ConnectionString;
        }
    }
}
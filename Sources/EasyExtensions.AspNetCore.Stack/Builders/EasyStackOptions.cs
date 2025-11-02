using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EasyExtensions.EntityFrameworkCore.Database;
using EasyExtensions.EntityFrameworkCore.HealthChecks;
using EasyExtensions.EntityFrameworkCore.Npgsql.Extensions;

namespace EasyExtensions.AspNetCore.Stack.Builders
{
    /// <summary>
    /// Provides configuration options for customizing EasyStack application features such as authorization and database
    /// setup.
    /// </summary>
    /// <remarks>Use this class to specify application-level options when integrating EasyStack into an
    /// ASP.NET Core application. The properties allow you to enable or disable authorization and to configure database
    /// services and health checks during startup. This class is typically configured during application initialization
    /// to tailor EasyStack's behavior to your application's requirements.</remarks>
    public class EasyStackOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether authorization should be added to the request.
        /// </summary>
        internal bool AuthorizationEnabled { get; set; }

        /// <summary>
        /// Enables authorization for the current EasyStackOptions instance.
        /// </summary>
        /// <returns>The current EasyStackOptions instance with authorization enabled.</returns>
        public EasyStackOptions AddAuthorization()
        {
            AuthorizationEnabled = true;
            return this;
        }

        /// <summary>
        /// Gets or sets the delegate used to configure database services and health checks during application startup.
        /// </summary>
        /// <remarks>Use this property to provide custom logic for registering database-related services
        /// and health checks with the application's dependency injection container. The delegate receives the service
        /// collection, health checks builder, and configuration, allowing for flexible setup of database connectivity
        /// and monitoring.</remarks>
        public Action<IServiceCollection, IHealthChecksBuilder, IConfiguration>? ConfigureDatabase { get; set; }

        /// <summary>
        /// Configures the application to use PostgreSQL as the database provider with the specified audited DbContext
        /// type.
        /// </summary>
        /// <remarks>This method registers the specified DbContext for use with PostgreSQL and adds a
        /// health check for the database connection. Lazy loading proxies are disabled by default for the registered
        /// DbContext.</remarks>
        /// <typeparam name="TDbContext">The type of the DbContext to use for PostgreSQL integration. Must inherit from AuditedDbContext.</typeparam>
        /// <returns>The current EasyStackOptions instance for method chaining.</returns>
        public EasyStackOptions WithPostgres<TDbContext>(bool useLazyLoadingProxies = false) where TDbContext : AuditedDbContext
        {
            ConfigureDatabase = (services, hc, cfg) =>
            {
                services.AddPostgresDbContext<TDbContext>(db =>
                {
                    db.UseLazyLoadingProxies = useLazyLoadingProxies;
                });

                hc.AddCheck<DatabaseHealthCheck<TDbContext>>("Database");
            };
            return this;
        }
    }
}

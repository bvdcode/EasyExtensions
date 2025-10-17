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
        public bool AddAuthorization { get; set; }

        /// <summary>
        /// Gets or sets the delegate used to configure database services and health checks during application startup.
        /// </summary>
        /// <remarks>Use this property to provide custom logic for registering database-related services
        /// and health checks with the application's dependency injection container. The delegate receives the service
        /// collection, health checks builder, and configuration, allowing for flexible setup of database connectivity
        /// and monitoring.</remarks>
        public Action<IServiceCollection, IHealthChecksBuilder, IConfiguration>? ConfigureDatabase { get; set; }


        public EasyStackOptions WithPostgres<TDbContext>() where TDbContext : AuditedDbContext
        {
            ConfigureDatabase = (services, hc, cfg) =>
            {
                services.AddPostgresDbContext<TDbContext>(db =>
                {
                    db.UseLazyLoadingProxies = false;
                });

                hc.AddCheck<DatabaseHealthCheck<TDbContext>>("Database");
            };
            return this;
        }
    }
}

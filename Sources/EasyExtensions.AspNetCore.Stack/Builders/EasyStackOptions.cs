// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.EntityFrameworkCore.Database;
using EasyExtensions.EntityFrameworkCore.HealthChecks;
using EasyExtensions.EntityFrameworkCore.Npgsql.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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
        /// Gets or sets a value indicating whether to use secrets for configuration values.
        /// </summary>
        internal bool UseSecretVault { get; set; }

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
        /// Gets or sets a delegate that configures SignalR hub options.
        /// </summary>
        /// <remarks>Use this property to customize SignalR behavior by modifying the provided <see
        /// cref="HubOptions"/> instance. This delegate is invoked during SignalR setup to apply additional
        /// configuration, such as setting maximum message size or enabling detailed errors.</remarks>
        public Action<HubOptions>? ConfigureSignalR { get; set; }

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

        /// <summary>
        /// Configures whether sensitive information should be stored in the secret vault.
        /// </summary>
        /// <param name="useSecrets">A value indicating whether to enable the use of the secret vault. Set to <see langword="true"/> to use the
        /// vault for sensitive information; otherwise, <see langword="false"/>.</param>
        /// <returns>The current instance of <see cref="EasyStackOptions"/> to allow for method chaining.</returns>
        public EasyStackOptions UseSecrets(bool useSecrets)
        {
            UseSecretVault = useSecrets;
            return this;
        }
    }
}

// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Abstractions;
using EasyExtensions.EntityFrameworkCore.Npgsql.Builders;
using EasyExtensions.EntityFrameworkCore.Npgsql.Migrations;
using EasyExtensions.EntityFrameworkCore.Npgsql.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
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
        public static IServiceCollection AddPostgresDbContext<TContext>(
            this IServiceCollection services,
            Action<PostgresOptionsBuilder>? setup = null,
            Action<DbContextOptionsBuilder>? setupContextOptions = null)
            where TContext : DbContext
        {
            PostgresOptionsBuilder contextFactory = new();
            setup?.Invoke(contextFactory);

            services.AddSingleton<IPostgresConnectionStringProvider>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                return new PostgresConnectionStringProvider(configuration, contextFactory);
            });
            services.AddDbContext<TContext>((sp, builder) =>
            {
                var conStringProvider = sp.GetRequiredService<IPostgresConnectionStringProvider>();
                builder.UseNpgsql(conStringProvider.GetConnectionString());
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

        /// <summary>
        /// Adds a <see cref="DbContext"/> to the <see cref="IServiceCollection"/> resolving <see cref="IConfiguration"/> from DI.
        /// Builds the connection string from the configured section (default: "DatabaseSettings") and/or prefixed keys.
        /// </summary>
        /// <typeparam name="TContext"> The type of <see cref="DbContext"/> to add. </typeparam>
        /// <typeparam name="TImplementation"> The implementation type of <see cref="DbContext"/> to add. </typeparam>
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
        public static IServiceCollection AddPostgresDbContext<TContext, TImplementation>(
            this IServiceCollection services,
            Action<PostgresOptionsBuilder>? setup = null,
            Action<DbContextOptionsBuilder>? setupContextOptions = null)
                where TContext : DbContext
                where TImplementation : TContext
        {
            PostgresOptionsBuilder contextFactory = new();
            setup?.Invoke(contextFactory);

            services.AddSingleton<IPostgresConnectionStringProvider>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                return new PostgresConnectionStringProvider(configuration, contextFactory);
            });
            services.AddDbContext<TContext, TImplementation>((sp, builder) =>
            {
                var conStringProvider = sp.GetRequiredService<IPostgresConnectionStringProvider>();
                builder.UseNpgsql(conStringProvider.GetConnectionString());
                setupContextOptions?.Invoke(builder);
                if (contextFactory.UseLazyLoadingProxies)
                {
                    builder.UseLazyLoadingProxies();
                }
            }, contextLifetime: contextFactory.ContextLifetime);

            if (contextFactory.AddDesignTimeDbContextFactory)
            {
                services.AddScoped<IDesignTimeDbContextFactory<TImplementation>, DesignTimeDbContextFactory<TImplementation>>();
            }
            return services;
        }
    }
}

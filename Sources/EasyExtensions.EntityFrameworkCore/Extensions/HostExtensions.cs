// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasyExtensions.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// <see cref="IHost"/> extensions.
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Applies migrations to the database.
        /// </summary>
        /// <returns> Current <see cref="IHost"/> instance. </returns>
        public static IHost ApplyMigrations<TContext>(this IHost host) where TContext : DbContext
        {
            using (var serviceScope = host.Services.GetService<IServiceScopeFactory>()!.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<TContext>();
                var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<DbContext>>();
                context.ApplyMigrations(logger);
            }
            return host;
        }
    }
}

// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyExtensions.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// Provides extension methods for applying database migrations to a DbContext instance.
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Applies all pending database migrations for the specified DbContext instance.
        /// </summary>
        /// <remarks>This method checks for any pending migrations and applies them in order. If there are
        /// no pending migrations, no action is taken. Logging is performed for each migration if a logger is
        /// provided.</remarks>
        /// <typeparam name="TContext">The type of the DbContext to apply migrations to.</typeparam>
        /// <param name="context">The DbContext instance whose pending migrations will be applied. Cannot be null.</param>
        /// <param name="logger">An optional logger used to record information about the migration process. If null, no logging is performed.</param>
        public static void ApplyMigrations<TContext>(this TContext context, ILogger? logger) where TContext : DbContext
        {
            var migrations = context.Database.GetPendingMigrations();
            if (migrations.Any())
            {
                foreach (var migration in migrations)
                {
                    logger?.LogInformation("Applying migration {Migration}.", migration);
                }
                context.Database.Migrate();
                logger?.LogInformation("Migrations applied.");
            }
        }
    }
}

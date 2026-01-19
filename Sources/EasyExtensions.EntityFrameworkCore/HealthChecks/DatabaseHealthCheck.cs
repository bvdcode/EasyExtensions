// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EasyExtensions.EntityFrameworkCore.HealthChecks
{
    /// <summary>
    /// Check if the database is available.
    /// </summary>
    /// <typeparam name="TContext">Type of the database context.</typeparam>
    public class DatabaseHealthCheck<TContext>(IServiceProvider _serviceProvider) : IHealthCheck where TContext : DbContext
    {
        /// <summary>
        /// Checks the health of the database by attempting to connect to it.
        /// </summary>
        /// <param name="context">Health check context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A HealthCheckResult indicating the health status of the database.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<TContext>();
            if (dbContext == null)
            {
                return HealthCheckResult.Unhealthy("Database context is not registered");
            }
            if (await dbContext.Database.CanConnectAsync(cancellationToken))
            {
                return HealthCheckResult.Healthy("Database is available");
            }

            return HealthCheckResult.Unhealthy("Database is unavailable");
        }
    }
}

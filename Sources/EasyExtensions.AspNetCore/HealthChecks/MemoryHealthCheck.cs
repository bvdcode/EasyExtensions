using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EasyExtensions.AspNetCore.HealthChecks
{
    /// <summary>
    /// Check if memory is available.
    /// </summary>
    public class MemoryHealthCheck : IHealthCheck
    {
        /// <summary>
        /// Threshold for memory usage, default is 0.9 (90%).
        /// </summary>
        public float Threshold { get; set; } = 0.9f;

        /// <summary>
        /// Runs the health check, returning the status of the component being checked.
        /// </summary>
        /// <param name="context">A context object associated with the current execution.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the health check.</param>
        /// <returns>A <see cref="Task{HealthCheckResult}"/> that completes when the health check has finished, yielding the status of the component being checked.</returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (Threshold <= 0 || Threshold >= 1)
            {
                throw new ArgumentOutOfRangeException(null, "Threshold must be between 0 and 1.");
            }

            long totalMemory = GC.GetTotalMemory(false);
            long availableMemory = GetAvailableMemory(totalMemory);
            long usedMemory = totalMemory - availableMemory;
            if (usedMemory < totalMemory * Threshold)
            {
                return Task.FromResult(HealthCheckResult.Healthy($"Memory usage is healthy: {usedMemory / 1024 / 1024} MB used"));
            }
            return Task.FromResult(HealthCheckResult.Unhealthy($"Memory usage is unhealthy: {usedMemory / 1024 / 1024} MB used"));
        }

        private static long GetAvailableMemory(long totalMemory)
        {
            var gcMemoryInfo = GC.GetGCMemoryInfo();
            long installedMemoryBytes = gcMemoryInfo.TotalAvailableMemoryBytes;
            long availableMemoryBytes = installedMemoryBytes - totalMemory;
            return availableMemoryBytes;
        }
    }
}

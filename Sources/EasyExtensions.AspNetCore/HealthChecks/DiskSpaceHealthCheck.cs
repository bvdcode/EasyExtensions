using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EasyExtensions.AspNetCore.HealthChecks
{
    /// <summary>
    /// Check if disk space is available.
    /// </summary>
    public class DiskSpaceHealthCheck : IHealthCheck
    {
        /// <summary>
        /// Minimum free space in MB.
        /// </summary>
        public int MinimumFreeSpaceMb { get; set; } = 100;

        /// <summary>
        /// Runs the health check, returning the status of the component being checked.
        /// </summary>
        /// <param name="context">A context object associated with the current execution.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the health check.</param>
        /// <returns>A <see cref="Task{HealthCheckResult}"/> that completes when the health check has finished, yielding the status of the component being checked.</returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (MinimumFreeSpaceMb <= 0)
            {
                throw new ArgumentOutOfRangeException(null, "Minimum free space must be greater than 0.");
            }
            string root = Path.GetPathRoot(Environment.CurrentDirectory) ?? throw new InvalidOperationException("Unable to determine root directory.");
            DriveInfo drive = new(root);
            if (drive.AvailableFreeSpace / 1024 / 1024 >= MinimumFreeSpaceMb)
            {
                return Task.FromResult(HealthCheckResult.Healthy($"Disk space is available: {drive.AvailableFreeSpace / 1024 / 1024} MB"));
            }
            return Task.FromResult(HealthCheckResult.Unhealthy($"Disk space is low: {drive.AvailableFreeSpace / 1024 / 1024} MB"));
        }
    }
}

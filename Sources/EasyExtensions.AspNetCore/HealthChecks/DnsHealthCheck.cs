using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.AspNetCore.HealthChecks
{
    /// <summary>
    /// Check if DNS resolver is available.
    /// </summary>
    public class DnsHealthCheck : IHealthCheck
    {
        /// <summary>
        /// DNS host to check, default is "example.com".
        /// </summary>
        public string Host { get; set; } = "example.com";

        /// <summary>
        /// Runs the health check, returning the status of the component being checked.
        /// </summary>
        /// <param name="context">A context object associated with the current execution.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the health check.</param>
        /// <returns>A <see cref="Task{HealthCheckResult}"/> that completes when the health check has finished, yielding the status of the component being checked.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            IPHostEntry entry = await Dns.GetHostEntryAsync(Host, cancellationToken);
            if (entry.AddressList.Length != 0)
            {
                return HealthCheckResult.Healthy("DNS resolver is available");
            }
            return HealthCheckResult.Unhealthy("DNS resolver is unavailable");
        }
    }
}

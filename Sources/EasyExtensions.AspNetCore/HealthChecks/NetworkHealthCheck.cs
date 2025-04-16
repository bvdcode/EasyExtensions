using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using EasyExtensions.Helpers;

namespace EasyExtensions.AspNetCore.HealthChecks
{
    /// <summary>
    /// Check if local network addresses are reachable.
    /// </summary>
    public class NetworkHealthCheck : IHealthCheck
    {
        /// <summary>
        /// Runs the health check, returning the status of the component being checked.
        /// </summary>
        /// <param name="context">A context object associated with the current execution.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the health check.</param>
        /// <returns>A <see cref="Task{HealthCheckResult}"/> that completes when the health check has finished, yielding the status of the component being checked.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddresses = host.AddressList;
            if (ipAddresses.Length == 0)
            {
                return HealthCheckResult.Unhealthy("No IP addresses found");
            }
            var tasks = ipAddresses.Select(PingAsync);
            var results = await Task.WhenAll(tasks);
            if (results.All(result => result))
            {
                return HealthCheckResult.Healthy("All IP addresses are reachable");
            }
            return HealthCheckResult.Unhealthy("Some IP addresses are unreachable");
        }

        private async static Task<bool> PingAsync(IPAddress ip)
        {
            return await NetworkHelpers.TryPingAsync(ip);
        }
    }
}

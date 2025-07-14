using System.Net;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using EasyExtensions.Models;
using EasyExtensions.Helpers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EasyExtensions.AspNetCore.HealthChecks
{
    /// <summary>
    /// Check if local network addresses are reachable.
    /// </summary>
    public class NetworkHealthCheck(ILogger<NetworkHealthCheck> _logger) : IHealthCheck
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
            var ipAddresses = host.AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip));
            if (!ipAddresses.Any())
            {
                return HealthCheckResult.Unhealthy("No IP addresses found");
            }
            List<IPAddress> unreachableIPs = [];
            foreach (var ip in ipAddresses)
            {
                IcmpResult result = await NetworkHelpers.TryPingAsync(ip, cancellationToken: cancellationToken);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Ping to {IP} failed with status {Status} and exception {Exception}", ip, result.Status, result.Exception?.Message);
                    unreachableIPs.Add(ip);
                }
            }
            if (unreachableIPs.Count == 0)
            {
                return HealthCheckResult.Healthy("All local network addresses are reachable");
            }
            else
            {
                string unreachableIPsString = string.Join(", ", unreachableIPs.Select(ip => ip.ToString()));
                return HealthCheckResult.Unhealthy($"The following local network addresses are unreachable: {unreachableIPsString}");
            }
        }
    }
}

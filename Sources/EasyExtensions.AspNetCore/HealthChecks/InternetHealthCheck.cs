using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EasyExtensions.AspNetCore.HealthChecks
{
    /// <summary>
    /// Check if a web page is available.
    /// </summary>
    public class InternetHealthCheck() : IHealthCheck
    {
        /// <summary>
        /// The URL to check, default is "https://example.com".
        /// </summary>
        public string Url { get; set; } = "https://example.com";

        /// <summary>
        /// Runs the health check, returning the status of the component being checked.
        /// </summary>
        /// <param name="context">A context object associated with the current execution.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the health check.</param>
        /// <returns>A <see cref="Task{HealthCheckResult}"/> that completes when the health check has finished, yielding the status of the component being checked.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(Url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy($"{Url} web page is available");
            }
            return HealthCheckResult.Unhealthy($"{Url} is unavailable. Status code: {response.StatusCode}");
        }
    }
}

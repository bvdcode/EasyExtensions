using EasyExtensions.Clients.Models;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace EasyExtensions.Clients
{
    /// <summary>
    /// Provides methods for retrieving geographic information based on IP addresses using the Splidex GeoIP service.
    /// </summary>
    public class GeoIpClient
    {
        private const string url = "https://geoip.splidex.com/";

        /// <summary>
        /// Asynchronously retrieves geolocation information for the current public IP address.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="GeoIpInfo"/> object
        /// with geolocation details for the current public IP address.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the response content cannot be deserialized into a <see cref="GeoIpInfo"/> object.</exception>
        public static Task<GeoIpInfo> LookupAsync(CancellationToken cancellationToken = default)
        {
            return LookupAsync(string.Empty, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves geographic information for the specified IP address.
        /// </summary>
        /// <param name="ip">The IP address to look up. Must be a valid IPv4 or IPv6 address.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="GeoIpInfo"/> object
        /// with geographic information for the specified IP address.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the response content cannot be deserialized into a <see cref="GeoIpInfo"/> object.</exception>
        public static async Task<GeoIpInfo> LookupAsync(string ip, CancellationToken cancellationToken = default)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url + ip, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync(GeoIpJsonSerializerContext.Default.GeoIpInfo, cancellationToken)
                ?? throw new InvalidOperationException("Failed to deserialize GeoIpInfo.");
        }
    }

    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(GeoIpInfo))]
    internal partial class GeoIpJsonSerializerContext : JsonSerializerContext
    {
    }
}

using EasyExtensions.Clients.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
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
        private const string currentHostCacheKey = "me";
        private const int cacheSizeLimit = 4096;
        private static readonly TimeSpan SlidingTtl = TimeSpan.FromDays(30);
        private static readonly TimeSpan AbsoluteTtl = TimeSpan.FromDays(90);

        private static readonly MemoryCache _cache = new(new MemoryCacheOptions
        {
            SizeLimit = cacheSizeLimit
        });

        private static readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions()
            .SetSize(1)
            .SetSlidingExpiration(SlidingTtl)
            .SetAbsoluteExpiration(AbsoluteTtl);

        private static readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri(url)
        };

        /// <summary>
        /// Asynchronously retrieves geolocation information for the current public IP address.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="GeoIpInfo"/> object
        /// with geolocation details for the current public IP address.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the response content cannot be deserialized into a <see cref="GeoIpInfo"/> object.</exception>
        public static Task<GeoIpInfo> LookupAsync(CancellationToken cancellationToken = default)
        {
            return LookupCoreAsync(currentHostCacheKey, string.Empty, cancellationToken);
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
            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new ArgumentException("IP address must be provided.", nameof(ip));
            }

            if (!IPAddress.TryParse(ip, out _))
            {
                throw new ArgumentException("Invalid IP address format.", nameof(ip));
            }

            return await LookupCoreAsync(ip, ip, cancellationToken);
        }

        /// <summary>
        /// Attempts to retrieve geolocation information for the current public IP address.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="GeoIpInfo"/>
        /// when lookup succeeds; otherwise <see langword="null"/>.</returns>
        public static async Task<GeoIpInfo?> TryLookupAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await LookupAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to retrieve geolocation information for the specified IP address.
        /// </summary>
        /// <param name="ip">The IP address to look up.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="GeoIpInfo"/>
        /// when lookup succeeds; otherwise <see langword="null"/>.</returns>
        public static async Task<GeoIpInfo?> TryLookupAsync(string ip, CancellationToken cancellationToken = default)
        {
            try
            {
                return await LookupAsync(ip, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return null;
            }
        }

        private static async Task<GeoIpInfo> LookupCoreAsync(string cacheKey, string requestPath, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(cacheKey, out GeoIpInfo? cachedResponse) && cachedResponse != null)
            {
                return cachedResponse;
            }

            var response = await _httpClient.GetAsync(requestPath, cancellationToken);
            response.EnsureSuccessStatusCode();

            var geoIpInfo = await response.Content.ReadFromJsonAsync(GeoIpJsonSerializerContext.Default.GeoIpInfo, cancellationToken)
                ?? throw new InvalidOperationException("Failed to deserialize GeoIpInfo.");

            _cache.Set(cacheKey, geoIpInfo, _cacheOptions);
            return geoIpInfo;
        }
    }

    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(GeoIpInfo))]
    internal partial class GeoIpJsonSerializerContext : JsonSerializerContext
    {
    }
}

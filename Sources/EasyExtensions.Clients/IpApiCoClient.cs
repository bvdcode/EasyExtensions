using EasyExtensions.Clients.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Http.Json;

namespace EasyExtensions.Clients
{
    /// <summary>
    /// Provides methods for retrieving geolocation and network information for IP addresses using the ipapi.co service.
    /// </summary>
    /// <remarks>Results are cached to improve performance and reduce the number of external API calls. If
    /// information for a given IP address is already cached, the cached result is returned instead of making a new
    /// request to the ipapi.co service. This class is not thread-safe for concurrent modifications to the cache beyond
    /// its intended usage pattern.</remarks>
    public static class IpApiCoClient
    {
        private const int MaxTTLDays = 7;
        private const int CacheSizeLimit = 10240;
        private static readonly MemoryCache _cache = new(new MemoryCacheOptions
        {
            SizeLimit = CacheSizeLimit
        });

        /// <summary>
        /// Initiates an asynchronous lookup of the public IP address and related information for the current machine.
        /// </summary>
        /// <remarks>This method performs a lookup without specifying an IP address, returning information
        /// about the caller's own public IP. To look up information for a specific IP address, use the overload that
        /// accepts an IP address parameter.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IpApiCoResponse"/>
        /// with details about the current public IP address.</returns>
        public static async Task<IpApiCoResponse> LookupCurrentHostAsync()
        {
            const string key = "me";
            if (_cache.TryGetValue(key, out IpApiCoResponse? cachedResponse) && cachedResponse != null)
            {
                return cachedResponse;
            }
            string url = $"https://ipapi.co/json/";
            using HttpClient http = new()
            {
                DefaultRequestHeaders =
                {
                    { "User-Agent", "EasyExtensions.Clients/1.0" }
                }
            };
            var response = await http.GetFromJsonAsync<IpApiCoResponse>(url)
                ?? throw new InvalidOperationException("Failed to get a valid response from ipapi.co.");
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSize(1)
                .SetSlidingExpiration(TimeSpan.FromDays(MaxTTLDays));
            _cache.Set(key, response, cacheEntryOptions);
            return response;
        }

        /// <summary>
        /// Asynchronously retrieves geolocation and network information for the specified IP address using the ipapi.co
        /// service.
        /// </summary>
        /// <remarks>Results are cached to improve performance and reduce external API calls. If the
        /// information for the specified IP address is already cached, the cached result is returned.</remarks>
        /// <param name="ip">The IPv4 or IPv6 address to look up. Must be a valid IP address in string format.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IpApiCoResponse"/>
        /// object with geolocation and network details for the specified IP address.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the ipapi.co service does not return a valid response.</exception>
        public static async Task<IpApiCoResponse> LookupAsync(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new ArgumentException("IP address must be provided.", nameof(ip));
            }
            if (!IPAddress.TryParse(ip, out _))
            {
                throw new ArgumentException("Invalid IP address format.", nameof(ip));
            }
            if (_cache.TryGetValue(ip, out IpApiCoResponse? cachedResponse) && cachedResponse != null)
            {
                return cachedResponse;
            }
            string url = $"https://ipapi.co/{ip}/json/";
            using HttpClient http = new()
            {
                DefaultRequestHeaders =
                {
                    { "User-Agent", "EasyExtensions.Clients/3.0" }
                }
            };
            var response = await http.GetFromJsonAsync<IpApiCoResponse>(url)
                ?? throw new InvalidOperationException("Failed to get a valid response from ipapi.co.");
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSize(1)
                .SetSlidingExpiration(TimeSpan.FromDays(MaxTTLDays));
            _cache.Set(ip, response, cacheEntryOptions);
            return response;
        }
    }
}

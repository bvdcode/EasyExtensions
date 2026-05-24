using EasyExtensions.Clients.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace EasyExtensions.Clients
{
    /// <summary>
    /// Provides methods for retrieving geographic information based on IP addresses using a GeoIP lookup service.
    /// </summary>
    public class GeoIpClient
    {
        private const string DefaultServerAddress = "https://geoip.splidex.com/";
        private const string CurrentHostCacheKey = "me";
        private const int CacheSizeLimit = 4096;
        private static readonly TimeSpan SlidingTtl = TimeSpan.FromDays(30);
        private static readonly TimeSpan AbsoluteTtl = TimeSpan.FromDays(90);

        private static readonly HttpClient SharedHttpClient = new();

        private static readonly MemoryCache _cache = new(new MemoryCacheOptions
        {
            SizeLimit = CacheSizeLimit
        });

        private static readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions()
            .SetSize(1)
            .SetSlidingExpiration(SlidingTtl)
            .SetAbsoluteExpiration(AbsoluteTtl);

        private readonly Uri _serverAddress;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Gets a shared client configured for the default Splidex GeoIP service.
        /// </summary>
        public static GeoIpClient Shared { get; } = new(new Uri(DefaultServerAddress), SharedHttpClient);

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoIpClient"/> class that uses the default Splidex GeoIP service.
        /// </summary>
        public GeoIpClient()
            : this(DefaultServerAddress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoIpClient"/> class that uses the specified GeoIP server address.
        /// </summary>
        /// <param name="serverAddress">
        /// The absolute GeoIP server address. It can point either to the service root or to a lookup endpoint, for example
        /// <c>https://bridge.cottoncloud.dev/api/v1/lookup</c>.
        /// </param>
        public GeoIpClient(string serverAddress)
            : this(CreateServerUri(serverAddress), SharedHttpClient)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoIpClient"/> class that uses the specified GeoIP server address.
        /// </summary>
        /// <param name="serverAddress">
        /// The absolute GeoIP server address. It can point either to the service root or to a lookup endpoint.
        /// </param>
        public GeoIpClient(Uri serverAddress)
            : this(serverAddress, SharedHttpClient)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoIpClient"/> class that uses the specified GeoIP server address
        /// and HTTP client.
        /// </summary>
        /// <param name="serverAddress">
        /// The absolute GeoIP server address. It can point either to the service root or to a lookup endpoint.
        /// </param>
        /// <param name="httpClient">The HTTP client used to send lookup requests.</param>
        public GeoIpClient(string serverAddress, HttpClient httpClient)
            : this(CreateServerUri(serverAddress), httpClient)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoIpClient"/> class that uses the specified GeoIP server address
        /// and HTTP client.
        /// </summary>
        /// <param name="serverAddress">
        /// The absolute GeoIP server address. It can point either to the service root or to a lookup endpoint.
        /// </param>
        /// <param name="httpClient">The HTTP client used to send lookup requests.</param>
        public GeoIpClient(Uri serverAddress, HttpClient httpClient)
        {
            _serverAddress = ValidateServerUri(serverAddress);
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Asynchronously retrieves geolocation information for the current public IP address.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="GeoIpInfo"/> object
        /// with geolocation details for the current public IP address.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the response content cannot be deserialized into a <see cref="GeoIpInfo"/> object.</exception>
        public Task<GeoIpInfo> LookupAsync(CancellationToken cancellationToken = default)
        {
            return LookupCoreAsync(CurrentHostCacheKey, string.Empty, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves geographic information for the specified IP address.
        /// </summary>
        /// <param name="ip">The IP address to look up. Must be a valid IPv4 or IPv6 address.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="GeoIpInfo"/> object
        /// with geographic information for the specified IP address.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the response content cannot be deserialized into a <see cref="GeoIpInfo"/> object.</exception>
        public async Task<GeoIpInfo> LookupAsync(string ip, CancellationToken cancellationToken = default)
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
        public async Task<GeoIpInfo?> TryLookupAsync(CancellationToken cancellationToken = default)
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
        public async Task<GeoIpInfo?> TryLookupAsync(string ip, CancellationToken cancellationToken = default)
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

        private async Task<GeoIpInfo> LookupCoreAsync(string lookupKey, string requestPath, CancellationToken cancellationToken)
        {
            var cacheKey = CreateCacheKey(_serverAddress, lookupKey);
            if (_cache.TryGetValue(cacheKey, out GeoIpInfo? cachedResponse) && cachedResponse != null)
            {
                return cachedResponse;
            }

            using var response = await _httpClient.GetAsync(CreateRequestUri(_serverAddress, requestPath), cancellationToken);
            response.EnsureSuccessStatusCode();

            var geoIpInfo = await response.Content.ReadFromJsonAsync(GeoIpJsonSerializerContext.Default.GeoIpInfo, cancellationToken)
                ?? throw new InvalidOperationException("Failed to deserialize GeoIpInfo.");

            _cache.Set(cacheKey, geoIpInfo, _cacheOptions);
            return geoIpInfo;
        }

        private static Uri CreateServerUri(string serverAddress)
        {
            if (string.IsNullOrWhiteSpace(serverAddress))
            {
                throw new ArgumentException("Server address must be provided.", nameof(serverAddress));
            }

            if (!Uri.TryCreate(serverAddress, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("Server address must be an absolute URI.", nameof(serverAddress));
            }

            return ValidateServerUri(uri);
        }

        private static Uri ValidateServerUri(Uri serverAddress)
        {
            ArgumentNullException.ThrowIfNull(serverAddress);

            if (!serverAddress.IsAbsoluteUri)
            {
                throw new ArgumentException("Server address must be an absolute URI.", nameof(serverAddress));
            }

            if (serverAddress.Scheme != Uri.UriSchemeHttp && serverAddress.Scheme != Uri.UriSchemeHttps)
            {
                throw new ArgumentException("Server address must use HTTP or HTTPS.", nameof(serverAddress));
            }

            return serverAddress;
        }

        private static Uri CreateRequestUri(Uri serverAddress, string requestPath)
        {
            if (string.IsNullOrEmpty(requestPath))
            {
                return serverAddress;
            }

            var builder = new UriBuilder(serverAddress);
            var basePath = builder.Path.TrimEnd('/');
            builder.Path = string.IsNullOrEmpty(basePath)
                ? requestPath
                : $"{basePath}/{requestPath}";

            return builder.Uri;
        }

        private static string CreateCacheKey(Uri serverAddress, string lookupKey)
        {
            return $"{serverAddress.AbsoluteUri}|{lookupKey}";
        }
    }

    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(GeoIpInfo))]
    internal partial class GeoIpJsonSerializerContext : JsonSerializerContext
    {
    }
}

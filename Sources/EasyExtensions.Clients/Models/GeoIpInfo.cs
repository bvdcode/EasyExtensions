using System.Text.Json.Serialization;

namespace EasyExtensions.Clients.Models
{
    /// <summary>
    /// Represents geographic and network information associated with an IP address, including country, region, city,
    /// autonomous system number (ASN), and ASN organization.
    /// </summary>
    /// <remarks>This record is typically used to provide location and network ownership details for IP-based
    /// lookups. All properties default to "Unknown" if the corresponding information is not available.</remarks>
    public record GeoIpInfo
    {
        /// <summary>
        /// Gets the country associated with the entity.
        /// </summary>
        [JsonPropertyName("country")]
        public string? Country { get; init; }

        /// <summary>
        /// Gets the region, state, or province associated with the entity.
        /// </summary>
        [JsonPropertyName("stateprov")]
        public string? Region { get; init; }

        /// <summary>
        /// Gets the region, state, or province code associated with the entity.
        /// </summary>
        [JsonPropertyName("stateprovCode")]
        public string? RegionCode { get; init; }

        /// <summary>
        /// Gets the name of the city associated with the entity.
        /// </summary>
        [JsonPropertyName("city")]
        public string? City { get; init; }

        /// <summary>
        /// Gets the autonomous system number (ASN) associated with the network entity.
        /// </summary>
        [JsonPropertyName("asn")]
        public int? Asn { get; init; }

        /// <summary>
        /// Gets the U.S. metro code associated with the entity.
        /// </summary>
        [JsonPropertyName("usMetroCode")]
        public int? UsMetroCode { get; init; }

        /// <summary>
        /// Gets the name of the organization associated with the autonomous system number (ASN).
        /// </summary>
        [JsonPropertyName("asnOrganization")]
        public string? AsnOrganization { get; init; }

        /// <summary>
        /// Gets the network associated with the autonomous system number (ASN).
        /// </summary>
        [JsonPropertyName("asnNetwork")]
        public string? AsnNetwork { get; init; }

        /// <summary>
        /// Gets the latitude coordinate in decimal degrees.
        /// </summary>
        [JsonPropertyName("latitude")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double? Latitude { get; init; }

        /// <summary>
        /// Gets the longitude coordinate in decimal degrees.
        /// </summary>
        [JsonPropertyName("longitude")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double? Longitude { get; init; }

        /// <summary>
        /// Gets the name of the continent associated with the entity.
        /// </summary>
        [JsonPropertyName("continent")]
        public string? Continent { get; init; }

        /// <summary>
        /// Gets the time zone identifier associated with the object.
        /// </summary>
        /// <remarks>The time zone identifier typically follows the IANA time zone database format (for
        /// example, "America/New_York"). The value may be null if no time zone is specified.</remarks>
        [JsonPropertyName("timezone")]
        public string? TimeZone { get; init; }

        /// <summary>
        /// Gets the radius of accuracy, in kilometers, for the associated location data.
        /// </summary>
        [JsonPropertyName("accuracyRadius")]
        public int? AccuracyRadius { get; init; }
    }
}

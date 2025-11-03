using System.Text.Json.Serialization;

namespace EasyExtensions.Clients.Models
{
    /// <summary>
    /// Represents the response data returned from the ip-api.co geolocation API for a given IP address.
    /// </summary>
    /// <remarks>This class provides strongly-typed access to various geographic, network, and country-related
    /// information associated with an IP address, as returned by the ip-api.co service. It is typically used to
    /// deserialize and work with API responses in applications that require IP-based geolocation or network
    /// metadata.</remarks>
    public class IpApiCoResponse
    {
        [JsonPropertyName("ip")]
        public string Ip { get; set; } = string.Empty;

        [JsonPropertyName("network")]
        public string Network { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("region")]
        public string Region { get; set; } = string.Empty;

        [JsonPropertyName("region_code")]
        public string RegionCode { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("country_name")]
        public string CountryName { get; set; } = string.Empty;

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; } = string.Empty;

        [JsonPropertyName("country_code_iso3")]
        public string CountryCodeISO3 { get; set; } = string.Empty;

        [JsonPropertyName("country_capital")]
        public string CountryCapital { get; set; } = string.Empty;

        [JsonPropertyName("country_tld")]
        public string CountryTld { get; set; } = string.Empty;

        [JsonPropertyName("continent_code")]
        public string ContinentCode { get; set; } = string.Empty;

        [JsonPropertyName("in_eu")]
        public bool InEU { get; set; }

        [JsonPropertyName("postal")]
        public string PostalCode { get; set; } = string.Empty;

        [JsonPropertyName("latitude")]
        public float? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public float? Longitude { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; } = string.Empty;

        [JsonPropertyName("utc_offset")]
        public string UtcOffset { get; set; } = string.Empty;

        [JsonPropertyName("country_calling_code")]
        public string CountryCallingCode { get; set; } = string.Empty;

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("currency_name")]
        public string CurrencyName { get; set; } = string.Empty;

        [JsonPropertyName("languages")]
        public string Languages { get; set; } = string.Empty;

        [JsonPropertyName("country_area")]
        public float CountryArea { get; set; }

        [JsonPropertyName("country_population")]
        public int CountryPopulation { get; set; }

        [JsonPropertyName("asn")]
        public string ASN { get; set; } = string.Empty;

        [JsonPropertyName("org")]
        public string Organization { get; set; } = string.Empty;
    }
}
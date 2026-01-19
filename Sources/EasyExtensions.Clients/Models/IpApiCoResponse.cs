// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

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
        /// <summary>
        /// Defines the IP address that was queried.
        /// </summary>
        [JsonPropertyName("ip")]
        public string Ip { get; set; } = string.Empty;

        /// <summary>
        /// The Network to which the IP address belongs.
        /// </summary>
        [JsonPropertyName("network")]
        public string Network { get; set; } = string.Empty;

        /// <summary>
        /// Indicates the version of the IP protocol (IPv4 or IPv6).
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Describes the city associated with the IP address.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Describes the region (state/province) associated with the IP address.
        /// </summary>
        [JsonPropertyName("region")]
        public string Region { get; set; } = string.Empty;

        /// <summary>
        /// Describes the region code associated with the IP address.
        /// </summary>
        [JsonPropertyName("region_code")]
        public string RegionCode { get; set; } = string.Empty;

        /// <summary>
        /// Describes the country associated with the IP address.
        /// </summary>
        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// The full name of the country associated with the IP address.
        /// </summary>
        [JsonPropertyName("country_name")]
        public string CountryName { get; set; } = string.Empty;

        /// <summary>
        /// The two-letter ISO code representing the country.
        /// </summary>
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; } = string.Empty;

        /// <summary>
        /// The three-letter ISO code representing the country.
        /// </summary>
        [JsonPropertyName("country_code_iso3")]
        public string CountryCodeISO3 { get; set; } = string.Empty;

        /// <summary>
        /// The capital city of the country associated with the IP address.
        /// </summary>
        [JsonPropertyName("country_capital")]
        public string CountryCapital { get; set; } = string.Empty;

        /// <summary>
        /// Denotes the top-level domain (TLD) for the country.
        /// </summary>
        [JsonPropertyName("country_tld")]
        public string CountryTld { get; set; } = string.Empty;

        /// <summary>
        /// Abbreviation for the continent where the country is located.
        /// </summary>
        [JsonPropertyName("continent_code")]
        public string ContinentCode { get; set; } = string.Empty;

        /// <summary>
        /// Specifies whether the country is a member of the European Union.
        /// </summary>
        [JsonPropertyName("in_eu")]
        public bool InEU { get; set; }

        /// <summary>
        /// Assigns the postal code associated with the IP address.
        /// </summary>
        [JsonPropertyName("postal")]
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Latitude coordinate of the IP address location.
        /// </summary>
        [JsonPropertyName("latitude")]
        public float? Latitude { get; set; }

        /// <summary>
        /// Longitude coordinate of the IP address location.
        /// </summary>
        [JsonPropertyName("longitude")]
        public float? Longitude { get; set; }

        /// <summary>
        /// Specifies the time zone of the IP address location.
        /// </summary>
        [JsonPropertyName("timezone")]
        public string Timezone { get; set; } = string.Empty;

        /// <summary>
        /// UTC offset for the IP address location.
        /// </summary>
        [JsonPropertyName("utc_offset")]
        public string UtcOffset { get; set; } = string.Empty;

        /// <summary>
        /// Country calling code (telephone dialing code).
        /// </summary>
        [JsonPropertyName("country_calling_code")]
        public string CountryCallingCode { get; set; } = string.Empty;

        /// <summary>
        /// Currency code used in the country.
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Currency name used in the country.
        /// </summary>
        [JsonPropertyName("currency_name")]
        public string CurrencyName { get; set; } = string.Empty;

        /// <summary>
        /// Languages spoken in the country.
        /// </summary>
        [JsonPropertyName("languages")]
        public string Languages { get; set; } = string.Empty;

        /// <summary>
        /// Country area in square kilometers.
        /// </summary>
        [JsonPropertyName("country_area")]
        public float CountryArea { get; set; }

        /// <summary>
        /// Population of the country.
        /// </summary>
        [JsonPropertyName("country_population")]
        public int CountryPopulation { get; set; }

        /// <summary>
        /// Autonomous System Number (ASN) associated with the IP address.
        /// </summary>
        [JsonPropertyName("asn")]
        public string ASN { get; set; } = string.Empty;

        /// <summary>
        /// The organization associated with the IP address.
        /// </summary>
        [JsonPropertyName("org")]
        public string Organization { get; set; } = string.Empty;
    }
}

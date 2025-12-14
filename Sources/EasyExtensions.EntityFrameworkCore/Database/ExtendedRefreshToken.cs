using EasyExtensions.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace EasyExtensions.EntityFrameworkCore.Database
{
    /// <summary>
    /// Represents a refresh token with extended metadata, including client device and location information, for
    /// authentication scenarios.
    /// </summary>
    /// <remarks>This class extends the base refresh token functionality by associating additional context
    /// such as IP address, user agent, authentication type, and geographic details. These properties can be used to
    /// enhance security, auditing, or analytics related to token usage. The class is mapped to the 'refresh_tokens'
    /// database table and enforces uniqueness on the token value.</remarks>
    [Table("refresh_tokens")]
    [Index(nameof(Token), IsUnique = true)]
    public class ExtendedRefreshToken : RefreshToken
    {
        /// <summary>
        /// Gets or sets the IP address associated with the entity.
        /// </summary>
        [Column("ip_address")]
        public IPAddress IpAddress { get; set; } = null!;

        /// <summary>
        /// Gets or sets the user agent string associated with the entity.
        /// </summary>
        [Column("user_agent")]
        public string UserAgent { get; set; } = null!;

        /// <summary>
        /// Gets or sets the authentication type used for this entity.
        /// </summary>
        [Column("auth_type")]
        public AuthType AuthType { get; set; }

        /// <summary>
        /// Gets or sets the country associated with the entity.
        /// </summary>
        [Column("country")]
        public string? Country { get; set; }

        /// <summary>
        /// Gets or sets the region associated with the entity.
        /// </summary>
        [Column("region")]
        public string? Region { get; set; }

        /// <summary>
        /// Gets or sets the name of the city associated with the entity.
        /// </summary>
        [Column("city")]
        public string? City { get; set; }

        /// <summary>
        /// Gets or sets the name or identifier of the device associated with this entity.
        /// </summary>
        [Column("device")]
        public string? Device { get; set; }
    }
}

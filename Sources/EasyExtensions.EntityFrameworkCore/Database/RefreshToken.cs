using EasyExtensions.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyExtensions.EntityFrameworkCore.Database
{
    /// <summary>
    /// Represents a refresh token used to obtain new access tokens for a user in authentication workflows.
    /// </summary>
    /// <remarks>A refresh token is typically issued alongside an access token and allows clients to request
    /// new access tokens without requiring the user to re-authenticate. Each refresh token is associated with a
    /// specific user and can be revoked to prevent further use.</remarks>
    [Table("refresh_tokens")]
    [Index(nameof(Token), IsUnique = true)]
    public class RefreshToken : BaseEntity<Guid>
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        [Column("user_id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the authentication token associated with the entity.
        /// </summary>
        [Column("token")]
        public string Token { get; set; } = null!;

        /// <summary>
        /// Gets or sets the date and time when the entity was revoked. Stored in UTC.
        /// </summary>
        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }
    }
}

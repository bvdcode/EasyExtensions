using System.Net;
using EasyExtensions.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyExtensions.EntityFrameworkCore.Database
{
    [Table("refresh_tokens")]
    [Index(nameof(Token), IsUnique = true)]
    internal class ExtendedRefreshToken : RefreshToken
    {
        [Column("ip_address")]
        public IPAddress IpAddress { get; set; } = null!;

        [Column("user_agent")]
        public string UserAgent { get; set; } = null!;

        [Column("auth_type")]
        public AuthType AuthType { get; set; }

        [Column("country")]
        public string? Country { get; set; }

        [Column("region")]
        public string? Region { get; set; }

        [Column("city")]
        public string? City { get; set; }

        [Column("device")]
        public string? Device { get; set; }
    }
}

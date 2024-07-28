using System.ComponentModel.DataAnnotations.Schema;

namespace EasyExtensions.EntityFrameworkCore.Abstractions
{
    /// <summary>
    /// Base model for user.
    /// </summary>
    public abstract class BaseUser : BaseEntity
    {
        /// <summary>
        /// User name.
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// User email.
        /// </summary>
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User password or hash.
        /// </summary>
        [Column("password")]
        public string Password { get; set; } = string.Empty;
    }
}

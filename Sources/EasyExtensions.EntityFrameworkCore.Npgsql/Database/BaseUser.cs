using EasyExtensions.EntityFrameworkCore.Abstractions;
using EasyExtensions.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasyExtensions.EntityFrameworkCore.Npgsql.Database
{
    /// <summary>
    /// Represents a base user account with identity, contact, and profile information.
    /// </summary>
    /// <remarks>This class provides common user properties for authentication, authorization, and
    /// personalization scenarios. It is intended to be used as a base type for user entities in applications that
    /// require user management. The class enforces unique email addresses and supports user preferences, permissions,
    /// and role-based access control.</remarks>
    [Table("users")]
    [Index(nameof(Email), IsUnique = true)]
    public class BasePostgresUser : BaseEntity<Guid>
    {
        /// <summary>
        /// Gets the full name, consisting of the first and last name combined with a space.
        /// </summary>
        [NotMapped]
        public string FullName => GetFullName();

        private string GetFullName()
        {
            var sb = new StringBuilder();
            sb.Append(FirstName);
            if (!string.IsNullOrWhiteSpace(MiddleName))
            {
                sb.Append(' ').Append(MiddleName);
            }
            sb.Append(' ').Append(LastName);
            return sb.ToString().Trim();
        }

        /// <summary>
        /// Gets or sets the first name of the person.
        /// </summary>
        [Column("first_name", TypeName = "citext")]
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the middle name of the person.
        /// </summary>
        [Column("middle_name", TypeName = "citext")]
        public string? MiddleName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the last name of the person.
        /// </summary>
        [Column("last_name", TypeName = "citext")]
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the avatar image data in WebP format.
        /// </summary>
        [Column("avatar_webp_bytes")]
        public byte[]? AvatarWebPBytes { get; set; }

        /// <summary>
        /// Gets or sets the email address associated with the entity.
        /// </summary>
        [Column("email", TypeName = "citext")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Gets or sets the phone number associated with the entity.
        /// </summary>
        [Column("phone_number")]
        public long? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the collection of user preferences as key-value pairs.
        /// </summary>
        /// <remarks>Each entry in the dictionary represents a user preference, where the key is the
        /// preference name and the value is its corresponding setting. Preference names and values are case-sensitive.
        /// Modifying this collection updates the user's stored preferences.</remarks>
        [Column("preferences")]
        public Dictionary<string, string> Preferences { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of permissions assigned to the entity.
        /// </summary>
        [Column("permissions")]
        public ICollection<string> Permissions { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether the entity is active.
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the password hash in PHC (Password Hashing Competition) string format.
        /// </summary>
        /// <remarks>The value is typically a string produced by a password hashing algorithm that follows
        /// the PHC string format, which includes information about the algorithm, parameters, salt, and hash. This
        /// property may be null if no password is set.</remarks>
        [Column("password_phc")]
        public string? PasswordPhc { get; set; }

        /// <summary>
        /// Gets or sets the role assigned to the user.
        /// </summary>
        [Column("role")]
        public UserRole Role { get; set; }

        /// <summary>
        /// Gets or sets the token used to authorize a password reset request for the user.
        /// </summary>
        [Column("reset_password_token", TypeName = "citext")]
        public string? ResetPasswordToken { get; set; }
    }
}

using EasyExtensions.Models.Enums;
using System;

namespace EasyExtensions.Models.Dto
{
    /// <summary>
    /// Represents the base data transfer object for a user, including common user profile information and role
    /// assignment.
    /// </summary>
    /// <remarks>This class provides fundamental user properties for use in API requests and responses. It is
    /// intended to be inherited by more specialized user DTOs as needed. The properties include identification, contact
    /// information, activation status, and role assignment.</remarks>
    public class BaseUserDto : BaseDto<Guid>
    {
        /// <summary>
        /// Gets or sets the first name of the person.
        /// </summary>
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the last name of the person.
        /// </summary>
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the email address associated with the user.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether the instance is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the role assigned to the user.
        /// </summary>
        public UserRole Role { get; set; }
    }
}

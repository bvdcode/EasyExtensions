using System.ComponentModel.DataAnnotations;

namespace EasyExtensions.AspNetCore.Authorization.Models.Dto
{
    /// <summary>
    /// Request model for changing a user's password.
    /// </summary>
    public record ChangePasswordRequestDto
    {
        /// <summary>
        /// The current password of the user.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string CurrentPassword { get; set; } = null!;

        /// <summary>
        /// The new password to set for the user.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string NewPassword { get; set; } = null!;
    }
}

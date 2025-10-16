using System.ComponentModel.DataAnnotations;

namespace EasyExtensions.AspNetCore.Authorization.Models.Dto
{
    /// <summary>
    /// Represents a request to refresh an authentication token using the current password.
    /// </summary>
    public record RefreshTokenRequestDto
    {
        /// <summary>
        /// Gets or sets the refresh token used to obtain a new access token when the current one expires.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string RefreshToken { get; set; } = null!;
    }
}

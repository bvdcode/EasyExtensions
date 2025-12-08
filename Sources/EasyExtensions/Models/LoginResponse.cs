namespace EasyExtensions.Models
{
    /// <summary>
    /// Represents the result of a successful authentication request, including access and refresh tokens.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Gets or sets the access token used for authenticating requests.
        /// </summary>
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// Gets or sets the refresh token used to obtain a new access token when the current token expires.
        /// </summary>
        public string? RefreshToken { get; set; }
    }
}

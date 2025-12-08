namespace EasyExtensions.Models
{
    /// <summary>
    /// Gets or sets the refresh token used to obtain a new access token.
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Gets or sets the refresh token used to obtain a new access token when the current access token expires.
        /// </summary>
        public string? RefreshToken { get; set; }
    }
}

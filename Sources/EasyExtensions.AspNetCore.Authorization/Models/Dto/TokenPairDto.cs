namespace EasyExtensions.AspNetCore.Authorization.Models.Dto
{
    /// <summary>
    /// Represents a pair of OAuth 2.0 tokens used for API authentication and token renewal.
    /// </summary>
    /// <remarks>A token pair typically consists of an access token, which is used to authorize API requests,
    /// and a refresh token, which can be used to obtain a new access token when the current one expires. This class is
    /// commonly used in authentication flows that require token management.</remarks>
    public class TokenPairDto
    {
        /// <summary>
        /// Gets or sets the OAuth 2.0 access token used for authenticating API requests.
        /// </summary>
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// Gets or sets the refresh token used to obtain a new access token when the current one expires.
        /// </summary>
        public string RefreshToken { get; set; } = null!;
    }
}

namespace EasyExtensions.AspNetCore.Authorization.Models.Dto.Enums
{
    /// <summary>
    /// Specifies the available authentication types for accessing a resource or service.
    /// </summary>
    public enum AuthType
    {
        /// <summary>
        /// Indicates that the content type is credentials, such as a username and password or authentication token.
        /// </summary>
        Credentials = 1,

        /// <summary>
        /// Specifies the Google authentication provider.
        /// </summary>
        Google = 2,
    }
}

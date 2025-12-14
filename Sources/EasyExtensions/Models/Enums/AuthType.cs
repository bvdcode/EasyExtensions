namespace EasyExtensions.Models.Enums
{
    /// <summary>
    /// Specifies the available authentication types for accessing a resource or service.
    /// </summary>
    public enum AuthType
    {
        /// <summary>
        /// Indicates that the value is unknown or has not been specified.
        /// </summary>
        /// <remarks>Use this value when the actual value is not available or cannot be determined. This
        /// is typically the default value for the enumeration.</remarks>
        Unknown = 0,

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

namespace EasyExtensions.Models
{
    /// <summary>
    /// Represents a request to authenticate a user using a username and password.
    /// </summary>
    public class UsernameLoginRequest
    {
        /// <summary>
        /// Gets or sets the username associated with the account.
        /// </summary>
        public string Username { get; set; } = null!;

        /// <summary>
        /// Gets or sets the password associated with the account or resource.
        /// </summary>
        public string Password { get; set; } = null!;
    }
}

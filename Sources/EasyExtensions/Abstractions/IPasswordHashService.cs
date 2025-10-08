namespace EasyExtensions.Abstractions
{
    /// <summary>
    /// Basic interface for a password hashing service.
    /// </summary>
    public interface IPasswordHashService
    {
        /// <summary>
        /// The version of the password hashing algorithm used.
        /// </summary>
        int PasswordHashVersion { get; }

        /// <summary>
        /// Hashes the given password and returns the password hash in PHC format.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The password hash in PHC format.</returns>
        string Hash(string password);

        /// <summary>
        /// Verifies the given password against the provided password hash.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="passwordHash">The password hash in PHC format to verify against.</param>
        /// <returns>True if the password matches the hash; otherwise, false.</returns>
        bool Verify(string password, string passwordHash);

        /// <summary>
        /// Verifies the given password against the provided password hash.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="passwordHash">The password hash in PHC format to verify against.</param>
        /// <param name="needsRehash">Outputs whether the password hash needs to be rehashed with the current algorithm version.</param>
        /// <returns>True if the password matches the hash; otherwise, false.</returns>
        bool Verify(string password, string passwordHash, out bool needsRehash);
    }
}
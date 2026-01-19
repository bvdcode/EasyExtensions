// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Abstractions
{
    /// <summary>
    /// Defines a user identity with a unique identifier, user name, and password hash in PHC format.
    /// </summary>
    /// <remarks>This interface abstracts the essential information required to represent a user identity in
    /// authentication and authorization scenarios. The password is stored as a PHC string, which is a standardized
    /// format for password hashes. Implementations should ensure that sensitive information, such as the password hash,
    /// is handled securely.</remarks>
    /// <typeparam name="TId">The type of the unique identifier for the user. This is typically a value type such as an integer or GUID.</typeparam>
    public interface IUserIdentity<out TId>
    {
        /// <summary>
        /// Gets the unique identifier for the entity.
        /// </summary>
        TId Id { get; }

        /// <summary>
        /// Gets the user name associated with the current context.
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Gets the password in PHC (Password Hashing Competition) string format.
        /// </summary>
        /// <remarks>The PHC string format encodes the password hash, algorithm identifier, and parameters
        /// in a standardized way for interoperability with PHC-compliant systems. This property is typically used for
        /// storing or verifying password hashes in a secure and portable manner.</remarks>
        string PasswordPhc { get; }
    }
}

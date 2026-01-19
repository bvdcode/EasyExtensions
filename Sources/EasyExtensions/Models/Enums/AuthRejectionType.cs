// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Models.Enums
{
    /// <summary>
    /// Specifies the reason for an authentication rejection.
    /// </summary>
    /// <remarks>Use this enumeration to determine the specific cause when an authentication attempt fails.
    /// The values indicate whether the failure was due to incorrect credentials, missing information, user not found,
    /// or other reasons. This can be useful for logging, auditing, or providing user feedback.</remarks>
    public enum AuthRejectionType
    {
        /// <summary>
        /// Indicates that the value is unknown or has not been specified.
        /// </summary>
        /// <remarks>Use this value when the actual value is not available or cannot be determined. This
        /// is typically the default value for the enumeration.</remarks>
        Unknown = 0,

        /// <summary>
        /// Indicates that no options are set.
        /// </summary>
        None = 1,

        /// <summary>
        /// Indicates that the operation failed due to an incorrect password.
        /// </summary>
        WrongPassword = 2,

        /// <summary>
        /// Indicates that no password is required for authentication.
        /// </summary>
        NoPassword = 3,

        /// <summary>
        /// Indicates that the user cannot log in using an external authentication provider.
        /// </summary>
        /// <remarks>This value is typically used to represent a login failure due to restrictions or
        /// issues with overriding validation inherited from an external authentication source - 'CanUserLoginAsync'</remarks>
        CannotLoginExternal = 4,

        /// <summary>
        /// Indicates that the specified user could not be found.
        /// </summary>
        UserNotFound = 5
    }
}

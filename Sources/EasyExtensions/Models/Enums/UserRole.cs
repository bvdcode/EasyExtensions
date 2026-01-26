// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Models.Enums
{
    /// <summary>
    /// Specifies the set of roles that can be assigned to a user within the system.
    /// </summary>
    /// <remarks>Use this enumeration to define user permissions and access levels. The roles represent
    /// increasing levels of responsibility and access, from standard users to administrators. The meaning and usage of
    /// the 'Custom' role may vary depending on application-specific requirements.</remarks>
    public enum UserRole
    {
        /// <summary>
        /// Indicates that no options are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents a standard user account with typical permissions and access rights.
        /// </summary>
        User = 1,

        /// <summary>
        /// Specifies an administrator role with elevated permissions.
        /// </summary>
        Admin = 2,
    }
}

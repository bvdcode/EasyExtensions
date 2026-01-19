// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.ComponentModel.DataAnnotations.Schema;

namespace EasyExtensions.EntityFrameworkCore.Abstractions
{
    /// <summary>
    /// Base model for user.
    /// </summary>
    public abstract class BaseUser<TId> : BaseEntity<TId> where TId : struct
    {
        /// <summary>
        /// User name.
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// User email.
        /// </summary>
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User password or hash.
        /// </summary>
        [Column("password")]
        public string Password { get; set; } = string.Empty;
    }
}

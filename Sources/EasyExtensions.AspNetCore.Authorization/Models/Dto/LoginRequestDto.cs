// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.ComponentModel.DataAnnotations;

namespace EasyExtensions.AspNetCore.Authorization.Models.Dto
{
    /// <summary>
    /// Represents the data required to perform a user login request, including the username and password.
    /// </summary>
    /// <remarks>This data transfer object is typically used to submit login credentials to an authentication
    /// endpoint. The <see cref="Username"/> property is required and must not be null or empty. The <see
    /// cref="Password"/> property should contain the user's password in plain text; ensure secure transmission and
    /// handling of this information.</remarks>
    public record class LoginRequestDto
    {
        /// <summary>
        /// Gets or sets the username associated with the user account.
        /// </summary>
        [Required]
        public string Username { get; set; } = null!;

        /// <summary>
        /// Gets or sets the password used for authentication.
        /// </summary>
        [Required]
        public string Password { get; set; } = null!;
    }
}

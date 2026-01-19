// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.AspNetCore.Authorization.Models.Dto
{
    /// <summary>
    /// Represents a request to refresh an authentication token using the current password.
    /// </summary>
    public record RefreshTokenRequestDto
    {
        /// <summary>
        /// Gets or sets the refresh token used to obtain a new access token when the current one expires.
        /// </summary>
        public string? RefreshToken { get; set; }
    }
}

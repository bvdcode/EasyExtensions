// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.AspNetCore.Authorization.Models
{
    internal class JwtSettings
    {
        internal int LifetimeMinutes { get; set; }
        internal string Key { get; set; } = string.Empty;
        internal string Issuer { get; set; } = string.Empty;
        internal string Audience { get; set; } = string.Empty;
        internal string Algorithm { get; set; } = string.Empty;
    }
}

// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Text.Json.Serialization;

namespace EasyExtensions.Passkeys.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<UserVerificationRequirement>))]
    public enum UserVerificationRequirement
    {
        [JsonStringEnumMemberName("required")] Required,
        [JsonStringEnumMemberName("preferred")] Preferred,
        [JsonStringEnumMemberName("discouraged")] Discouraged
    }
}

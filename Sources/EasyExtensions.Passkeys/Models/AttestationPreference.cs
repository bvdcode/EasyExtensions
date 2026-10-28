// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Text.Json.Serialization;

namespace EasyExtensions.Passkeys.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<AttestationPreference>))]
    public enum AttestationPreference
    {
        [JsonStringEnumMemberName("none")] None,
        [JsonStringEnumMemberName("direct")] Direct,
        [JsonStringEnumMemberName("indirect")] Indirect,
        [JsonStringEnumMemberName("enterprise")] Enterprise
    }
}

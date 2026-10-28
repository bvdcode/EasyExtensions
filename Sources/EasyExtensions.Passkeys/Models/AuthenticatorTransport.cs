// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Text.Json.Serialization;

namespace EasyExtensions.Passkeys.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<AuthenticatorTransport>))]
    public enum AuthenticatorTransport
    {
        [JsonStringEnumMemberName("usb")] Usb,
        [JsonStringEnumMemberName("nfc")] Nfc,
        [JsonStringEnumMemberName("ble")] Ble,
        [JsonStringEnumMemberName("internal")] Internal,
        [JsonStringEnumMemberName("hybrid")] Hybrid,
        [JsonStringEnumMemberName("smart-card")] SmartCard
    }
}

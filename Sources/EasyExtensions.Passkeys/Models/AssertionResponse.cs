// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Passkeys.Models
{
    public record AssertionResponse(byte[] ClientDataJSON, byte[] AuthenticatorData, byte[] Signature)
    {
        public byte[]? UserHandle { get; init; }
    }
}

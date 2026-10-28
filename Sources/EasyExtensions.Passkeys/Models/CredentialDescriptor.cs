// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Passkeys.Models
{
    public record CredentialDescriptor(byte[] Id)
    {
        public string Type => "public-key";
        public IReadOnlyList<AuthenticatorTransport> Transports { get; init; } = [];
    }
}

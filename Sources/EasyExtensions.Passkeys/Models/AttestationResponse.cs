// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Passkeys.Models
{
    public record AttestationResponse(byte[] ClientDataJSON, byte[] AttestationObject)
    {
        public IReadOnlyList<AuthenticatorTransport> Transports { get; init; } = [];
    }
}

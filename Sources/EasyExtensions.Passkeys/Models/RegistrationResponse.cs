// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Passkeys.Models
{
    public record RegistrationResponse(string Id, byte[] RawId, string Type, AttestationResponse Response);
}

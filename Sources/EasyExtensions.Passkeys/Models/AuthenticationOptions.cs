// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Passkeys.Models
{
    public record AuthenticationOptions(
        string RpId,
        byte[] Challenge,
        IReadOnlyList<CredentialDescriptor> AllowCredentials,
        UserVerificationRequirement UserVerification,
        int Timeout);
}

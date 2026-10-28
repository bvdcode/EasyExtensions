// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Passkeys.Models
{
    public record RegistrationResult(
        byte[] CredentialId,
        byte[] CosePublicKey,
        byte[] UserHandle,
        uint SignatureCounter,
        Guid AaGuid,
        bool IsBackupEligible,
        bool IsBackedUp,
        string AttestationFormat,
        AttestationTrust Trust,
        IReadOnlyList<AuthenticatorTransport> Transports);
}

// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Passkeys.Models
{
    public record RegistrationOptions(
        RelyingParty Rp,
        PasskeyUser User,
        byte[] Challenge,
        IReadOnlyList<CredentialParameter> PubKeyCredParams,
        IReadOnlyList<CredentialDescriptor> ExcludeCredentials,
        AuthenticatorSelection AuthenticatorSelection,
        AttestationPreference Attestation,
        int Timeout);
}

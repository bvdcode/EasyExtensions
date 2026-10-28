// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Passkeys.Cryptography;
using EasyExtensions.Passkeys.Protocol;

namespace EasyExtensions.Passkeys.Attestation
{
    internal record AttestationContext(byte[] AuthenticatorData, byte[] ClientDataHash, AuthenticatorData Credential)
    {
        internal byte[] SignedData => [.. AuthenticatorData, .. ClientDataHash];
        internal CoseKey Key => Credential.Key ?? throw new PasskeyVerificationException("Missing attested public key.");
    }
}

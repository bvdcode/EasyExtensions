// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys
{
    public record PasskeyServerOptions(string RelyingPartyId, string RelyingPartyName, IReadOnlyCollection<string> AllowedOrigins)
    {
        public UserVerificationRequirement UserVerification { get; init; } = UserVerificationRequirement.Required;
        public ResidentKeyRequirement ResidentKey { get; init; } = ResidentKeyRequirement.Required;
        public AttestationPreference Attestation { get; init; } = AttestationPreference.Direct;
        public IReadOnlyCollection<CoseAlgorithm> Algorithms { get; init; } =
            [CoseAlgorithm.ES256, CoseAlgorithm.RS256, CoseAlgorithm.EdDSA];
        public int TimeoutMilliseconds { get; init; } = 60_000;
        public bool RequireTrustedAttestation { get; init; }
        public IReadOnlyCollection<byte[]> AttestationTrustRoots { get; init; } = [];
    }
}

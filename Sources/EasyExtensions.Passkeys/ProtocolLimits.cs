// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Passkeys
{
    internal static class ProtocolLimits
    {
        internal const int ChallengeLength = 32;
        internal const int MaximumClientDataLength = 16 * 1024;
        internal const int MaximumAttestationLength = 256 * 1024;
        internal const int MaximumEncodedResponseLength = 512 * 1024;
        internal const int MaximumCredentialIdLength = 1023;
        internal const int MaximumPublicKeyLength = 8192;
        internal const int MaximumCertificateCount = 8;
        internal const int MaximumCertificateLength = 16 * 1024;
    }
}

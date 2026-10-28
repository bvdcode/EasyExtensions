// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Passkeys.Models
{
    public enum CoseAlgorithm
    {
        ES256 = -7,
        EdDSA = -8,
        ES384 = -35,
        ES512 = -36,
        PS256 = -37,
        PS384 = -38,
        PS512 = -39,
        ES256K = -47,
        RS256 = -257,
        RS384 = -258,
        RS512 = -259
    }
}

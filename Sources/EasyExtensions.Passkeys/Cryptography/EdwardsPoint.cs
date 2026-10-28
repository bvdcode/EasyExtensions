// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Numerics;

namespace EasyExtensions.Passkeys.Cryptography
{
    internal readonly record struct EdwardsPoint(BigInteger X, BigInteger Y, BigInteger Z, BigInteger T);
}

// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;

namespace EasyExtensions.Crypto.Internals
{
    internal static class FormatConstants
    {
        public const int Version = 1;
        public static ReadOnlySpan<byte> MagicBytes => "CTN1"u8;
    }
}

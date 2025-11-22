// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using System;

namespace EasyExtensions.Crypto.Internals
{
    internal static class FormatConstants
    {
        public const int Version = 1;
        public static ReadOnlySpan<byte> MagicBytes => "CTN1"u8;
    }
}

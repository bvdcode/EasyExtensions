// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using System;
using System.Buffers.Binary;

namespace EasyExtensions.Crypto.Internals
{
    // 128-bit tag stored without heap allocations
    internal readonly struct Tag128(ulong lo, ulong hi)
    {
        public ulong Lo { get; } = lo;
        public ulong Hi { get; } = hi;

        public static Tag128 FromSpan(ReadOnlySpan<byte> tagBytes)
        {
            if (tagBytes.Length < 16) throw new ArgumentException("Tag span must be 16 bytes", nameof(tagBytes));
            ulong lo = BinaryPrimitives.ReadUInt64LittleEndian(tagBytes[..8]);
            ulong hi = BinaryPrimitives.ReadUInt64LittleEndian(tagBytes.Slice(8, 8));
            return new Tag128(lo, hi);
        }

        public void CopyTo(Span<byte> destination)
        {
            if (destination.Length < 16) throw new ArgumentException("Destination span must be 16 bytes", nameof(destination));
            BinaryPrimitives.WriteUInt64LittleEndian(destination[..8], Lo);
            BinaryPrimitives.WriteUInt64LittleEndian(destination.Slice(8, 8), Hi);
        }
    }
}

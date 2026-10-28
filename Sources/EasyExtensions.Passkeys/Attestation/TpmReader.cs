// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Buffers.Binary;

namespace EasyExtensions.Passkeys.Attestation
{
    internal class TpmReader(byte[] data)
    {
        private int position;

        internal ushort UInt16() => BinaryPrimitives.ReadUInt16BigEndian(Read(2));
        internal uint UInt32() => BinaryPrimitives.ReadUInt32BigEndian(Read(4));
        internal byte[] SizedBytes() => Read(UInt16()).ToArray();

        internal ReadOnlySpan<byte> Read(int count)
        {
            if (count < 0 || count > data.Length - position)
            {
                throw new PasskeyVerificationException("Truncated TPM structure.");
            }
            ReadOnlySpan<byte> result = data.AsSpan(position, count);
            position += count;
            return result;
        }

        internal void End()
        {
            if (position != data.Length)
            {
                throw new PasskeyVerificationException("Trailing TPM structure data.");
            }
        }

        internal void Symmetric()
        {
            ushort algorithm = UInt16();
            switch (algorithm)
            {
                case 0x0010:
                    return;
                case 0x0006:
                case 0x0013:
                case 0x0026:
                    _ = UInt16();
                    _ = UInt16();
                    return;
                default:
                    throw new PasskeyVerificationException("Unsupported TPM symmetric algorithm.");
            }
        }

        internal void Scheme()
        {
            ushort algorithm = UInt16();
            switch (algorithm)
            {
                case 0x0010:
                case 0x0015:
                    return;
                case 0x0014:
                case 0x0016:
                case 0x0017:
                case 0x0018:
                case 0x0019:
                case 0x001B:
                case 0x001C:
                case 0x0020:
                case 0x0021:
                case 0x0022:
                    _ = UInt16();
                    return;
                case 0x001A:
                    _ = UInt16();
                    _ = UInt16();
                    return;
                default:
                    throw new PasskeyVerificationException("Unsupported TPM scheme.");
            }
        }
    }
}

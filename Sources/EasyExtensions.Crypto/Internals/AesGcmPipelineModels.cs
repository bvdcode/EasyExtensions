// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Crypto.Internals
{
    // Work queue and results for encryption pipeline
    internal readonly struct EncryptionJob(long index, byte[] dataBuffer, int dataLength)
    {
        public long Index { get; } = index;
        public byte[] DataBuffer { get; } = dataBuffer;
        public int DataLength { get; } = dataLength;
    }

    internal readonly struct EncryptionResult(long index, Tag128 tag, byte[] data, int dataLength)
    {
        public long Index { get; } = index;
        public Tag128 Tag { get; } = tag; // 16-byte tag, value type
        public byte[] Data { get; } = data; // ciphertext buffer, rented from pool
        public int DataLength { get; } = dataLength;
    }

    // Work queue and results for decryption pipeline
    internal readonly struct DecryptionJob(long index, Tag128 tag, byte[] cipherBuffer, int dataLength)
    {
        public long Index { get; } = index;
        public Tag128 Tag { get; } = tag; // 16-byte tag value
        public byte[] Cipher { get; } = cipherBuffer; // rented from pool
        public int DataLength { get; } = dataLength;
    }

    internal readonly struct DecryptionResult(long index, byte[] data, int dataLength)
    {
        public long Index { get; } = index;
        public byte[] Data { get; } = data; // rented from pool
        public int DataLength { get; } = dataLength;
    }
}

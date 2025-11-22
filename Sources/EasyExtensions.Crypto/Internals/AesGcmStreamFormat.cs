// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using System;
using System.IO;
using System.Buffers;
using System.Threading;
using System.Buffers.Binary;
using System.Threading.Tasks;
using EasyExtensions.Crypto.Internals;

namespace EasyExtensions.Crypto.Internals
{
    // FileHeader and ChunkHeader moved to Headers.cs

    internal static class AesGcmStreamFormat
    {
        // Keep version number via FormatConstants
        private static ReadOnlySpan<byte> MagicBytes => FormatConstants.MagicBytes;

        /// <summary>
        /// Compose the 12-byte nonce as: [4 bytes file prefix][8 bytes chunk counter].
        /// The chunk counter space is 64-bit. To avoid IV reuse, the maximum number of chunks per file is 2^64 - 1.
        /// If the counter equals ulong.MaxValue, this method throws InvalidOperationException.
        /// </summary>
        /// <param name="destination">Destination 12-byte buffer.</param>
        /// <param name="fileNoncePrefix">Per-file 4-byte prefix.</param>
        /// <param name="chunkIndex">Zero-based chunk index.</param>
        public static void ComposeNonce(Span<byte> destination, uint fileNoncePrefix, long chunkIndex)
        {
            // Guard: prevent wrapping the 64-bit counter in the nonce
            if (unchecked((ulong)chunkIndex) == ulong.MaxValue)
            {
                throw new InvalidOperationException("Maximum number of chunks per file is 2^64-1. Counter reached ulong.MaxValue.");
            }

            BinaryPrimitives.WriteUInt32LittleEndian(destination, fileNoncePrefix);
            BinaryPrimitives.WriteUInt64LittleEndian(destination[4..], unchecked((ulong)chunkIndex));
        }

        public static void InitAadPrefix(Span<byte> aad32, int keyId)
        {
            if (aad32.Length < 32) throw new ArgumentException("AAD buffer must be at least 32 bytes", nameof(aad32));
            MagicBytes.CopyTo(aad32[..4]);
            BinaryPrimitives.WriteInt32LittleEndian(aad32.Slice(4, 4), 1); // version stays 1
            BinaryPrimitives.WriteInt32LittleEndian(aad32.Slice(8, 4), keyId);
        }

        public static void FillAadMutable(Span<byte> aad32, long chunkIndex, long plainLength)
        {
            if (aad32.Length < 32) throw new ArgumentException("AAD buffer must be at least 32 bytes", nameof(aad32));
            BinaryPrimitives.WriteInt64LittleEndian(aad32.Slice(12, 8), chunkIndex);
            BinaryPrimitives.WriteInt64LittleEndian(aad32.Slice(20, 8), plainLength);
            BinaryPrimitives.WriteInt32LittleEndian(aad32.Slice(28, 4), 0);
        }

        public static int ComputeFileHeaderLength(int nonceSize, int tagSize, int keySize)
            => FileHeader.ComputeLength(nonceSize, tagSize, keySize);

        public static void BuildFileHeader(Span<byte> header, int keyId, uint noncePrefix, ReadOnlySpan<byte> fileKeyNonce, Tag128 fileKeyTag, ReadOnlySpan<byte> encryptedFileKey, long totalPlaintextLength, int nonceSize, int tagSize, int keySize)
        {
            var fh = new FileHeader(keyId, noncePrefix, fileKeyNonce.ToArray(), fileKeyTag, encryptedFileKey.ToArray(), totalPlaintextLength);
            if (!FileHeader.TryWrite(header, fh, nonceSize, tagSize, keySize))
                throw new ArgumentException("Header buffer too small", nameof(header));
        }

        public static int ComputeChunkHeaderLength(int tagSize)
            => ChunkHeader.ComputeLength(tagSize); // magic + headerLen + plainLen + keyId + tag

        public static void BuildChunkHeader(Span<byte> header, int keyId, Tag128 tag, int textLength, int tagSize)
        {
            int required = ComputeChunkHeaderLength(tagSize);
            if (header.Length < required) throw new ArgumentException("Header buffer too small", nameof(header));
            int offset = 0;
            MagicBytes.CopyTo(header[offset..]);
            offset += MagicBytes.Length;
            BinaryPrimitives.WriteInt32LittleEndian(header[offset..], required);
            offset += sizeof(int);
            BinaryPrimitives.WriteInt64LittleEndian(header[offset..], (long)textLength);
            offset += sizeof(long);
            BinaryPrimitives.WriteInt32LittleEndian(header[offset..], keyId);
            offset += sizeof(int);
            // write tag
            tag.CopyTo(header[offset..(offset + tagSize)]);
        }

        public static async Task<FileHeader> ReadFileHeaderAsync(Stream input, int nonceSize, int tagSize, int keySize, CancellationToken ct)
        {
            byte[] headerPrefix = ArrayPool<byte>.Shared.Rent(8);
            try
            {
                await ReadExactlyAsync(input, headerPrefix, 8, ct).ConfigureAwait(false);
                if (!headerPrefix.AsSpan(0, 4).SequenceEqual(MagicBytes))
                {
                    throw new InvalidDataException("Invalid file format: magic header not found.");
                }
                int headerLength = BinaryPrimitives.ReadInt32LittleEndian(headerPrefix.AsSpan(4));
                if (headerLength != ComputeFileHeaderLength(nonceSize, tagSize, keySize))
                {
                    throw new InvalidDataException("Unsupported file header format (unexpected header length).");
                }
                int remainingHeader = headerLength - 8;
                byte[] headerData = ArrayPool<byte>.Shared.Rent(remainingHeader);
                try
                {
                    await ReadExactlyAsync(input, headerData, remainingHeader, ct).ConfigureAwait(false);
                    // Stitch full header then parse via internal TryRead
                    byte[] full = ArrayPool<byte>.Shared.Rent(headerLength);
                    try
                    {
                        headerPrefix.AsSpan(0, 8).CopyTo(full);
                        headerData.AsSpan(0, remainingHeader).CopyTo(full.AsSpan(8));
                        if (!FileHeader.TryRead(full, nonceSize, tagSize, keySize, out var fh))
                            throw new InvalidDataException("Invalid file header contents.");
                        return fh;
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(full, clearArray: false);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(headerData, clearArray: false);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(headerPrefix, clearArray: false);
            }
        }

        public static async Task<ChunkHeader> ReadChunkHeaderAsync(Stream input, int tagSize, CancellationToken ct)
        {
            int headerLen = ComputeChunkHeaderLength(tagSize);
            byte[] header = ArrayPool<byte>.Shared.Rent(headerLen);
            try
            {
                await ReadExactlyAsync(input, header, headerLen, ct).ConfigureAwait(false);
                // Explicit magic check for fast-fail on wrong magic
                if (!header.AsSpan(0, 4).SequenceEqual(MagicBytes))
                {
                    throw new InvalidDataException("Invalid or unsupported chunk magic.");
                }
                if (!ChunkHeader.TryRead(header, tagSize, out var ch))
                {
                    throw new InvalidDataException("Invalid or corrupted chunk header.");
                }
                return ch;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(header, clearArray: false);
            }
        }

        public static async Task ReadExactlyAsync(Stream stream, byte[] buffer, int count, CancellationToken ct)
        {
            int offset = 0;
            while (offset < count)
            {
                int bytesRead = await stream.ReadAsync(buffer.AsMemory(offset, count - offset), ct).ConfigureAwait(false);
                if (bytesRead == 0)
                {
                    throw new EndOfStreamException("Unexpected end of stream.");
                }
                offset += bytesRead;
            }
        }
    }
}

// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;

namespace EasyExtensions.Crypto.Models
{
    /// <summary>
    /// Represents a zero-copy chunk of bytes for use in channel-based pipelines. The receiver is responsible for
    /// returning the buffer to the shared pool after use.
    /// </summary>
    /// <remarks>This struct is intended for high-performance scenarios where minimizing memory allocations is
    /// important. Ownership of the buffer is transferred to the receiver, who must ensure the buffer is returned to the
    /// appropriate pool or resource manager when no longer needed.</remarks>
    /// <param name="buffer">The byte array that backs the chunk. Cannot be null. The caller is responsible for ensuring the buffer remains
    /// valid for the lifetime of the chunk.</param>
    /// <param name="length">The number of valid bytes in the buffer. Must be non-negative and less than or equal to the length of the
    /// buffer.</param>
    public readonly struct ByteChunk(byte[] buffer, int length)
    {
        /// <summary>
        /// Gets the underlying byte array buffer associated with this instance.
        /// </summary>
        public byte[] Buffer { get; } = buffer ?? throw new ArgumentNullException(nameof(buffer));

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Length { get; } = length;
    }
}

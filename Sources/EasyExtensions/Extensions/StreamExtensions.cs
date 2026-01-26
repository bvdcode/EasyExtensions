// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace EasyExtensions
{
    /// <summary>
    /// <see cref="Stream"/> extensions.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads the bytes from the current stream and writes them to the byte array.
        /// </summary>
        /// <param name="stream"> The stream to read from. </param>
        /// <param name="leaveOpen"> If true, the stream will not be disposed after reading. </param>
        /// <returns> Received byte array. </returns>
        /// <exception cref="IOException"> An I/O error occurred. </exception>
        /// <exception cref="ArgumentNullException"> Destination is null. </exception>
        /// <exception cref="ObjectDisposedException"> Either the current stream or the destination stream is disposed. </exception>
        /// <exception cref="NotSupportedException"> The current stream does not support reading, or the destination stream does not support writing. </exception>
        public static byte[] ReadToEnd(this Stream stream, bool leaveOpen = false)
        {
            using MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            if (!leaveOpen)
            {
                stream.Dispose();
            }
            return ms.ToArray();
        }

        /// <summary>
        /// Asynchronously reads the bytes from the current stream and writes them to the byte array.
        /// </summary>
        /// <param name="stream"> The stream to read from. </param>
        /// <param name="leaveOpen"> If true, the stream will not be disposed after reading. </param>
        /// <returns> Received byte array. </returns>
        /// <exception cref="ArgumentNullException"> Destination is null. </exception>
        /// <exception cref="ObjectDisposedException"> Either the current stream or the destination stream is disposed. </exception>
        /// <exception cref="NotSupportedException"> The current stream does not support reading, or the destination stream does not support writing. </exception>
        public static async Task<byte[]> ReadToEndAsync(this Stream stream, bool leaveOpen = false)
        {
            using MemoryStream ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            if (!leaveOpen)
            {
                await stream.DisposeAsync();
            }
            return ms.ToArray();
        }

        /// <summary>
        /// Computes the SHA-256 hash of the data in the specified stream and returns it as a lowercase hexadecimal
        /// string.
        /// </summary>
        /// <remarks>The method reads from the current position of the stream to the end. After the
        /// operation, the stream's position will be at the end of the stream. The caller is responsible for managing
        /// the stream's position and lifetime.</remarks>
        /// <param name="stream">The input stream containing the data to hash. The stream must be readable and positioned at the start of the
        /// data to hash.</param>
        /// <returns>A lowercase hexadecimal string representation of the SHA-256 hash of the stream's contents.</returns>
        public static string Sha256(this Stream stream)
        {
            using SHA256 sha = SHA256.Create();
            byte[] array = sha.ComputeHash(stream);
            return array.ToHexStringLower();
        }

        /// <summary>
        /// Computes the SHA-512 hash of the data in the specified stream and returns it as a lowercase hexadecimal
        /// string.
        /// </summary>
        /// <remarks>The method does not reset the position of the stream after computing the hash. The
        /// caller is responsible for managing the stream's position and lifetime.</remarks>
        /// <param name="stream">The input stream containing the data to hash. The stream must be readable and positioned at the start of the
        /// data to hash.</param>
        /// <returns>A lowercase hexadecimal string representing the SHA-512 hash of the stream's contents.</returns>
        public static string Sha512(this Stream stream)
        {
            using SHA512 sha = SHA512.Create();
            byte[] array = sha.ComputeHash(stream);
            return array.ToHexStringLower();
        }
    }
}

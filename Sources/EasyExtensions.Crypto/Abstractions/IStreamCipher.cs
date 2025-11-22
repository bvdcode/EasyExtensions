// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Crypto.Abstractions
{
    /// <summary>
    /// Defines methods for encrypting and decrypting data streams using a stream cipher algorithm.
    /// </summary>
    /// <remarks>Implementations of this interface provide asynchronous operations for encrypting and
    /// decrypting data directly from streams. Methods support processing large data in chunks and allow control over
    /// stream ownership. This interface is intended for use with stream-based encryption scenarios, such as file or
    /// network data processing.</remarks>
    public interface IStreamCipher
    {
        /// <summary>
        /// Asynchronously encrypts data from the input stream and writes the encrypted data to the output stream using
        /// AES-GCM encryption.
        /// </summary>
        /// <remarks>The method does not reset the position of the input or output streams. The caller is
        /// responsible for managing stream positions as needed. If the operation is canceled via the cancellation
        /// token, the output stream may contain partial data.</remarks>
        /// <param name="input">The stream containing the plaintext data to encrypt. Must be readable.</param>
        /// <param name="output">The stream to which the encrypted data will be written. Must be writable.</param>
        /// <param name="chunkSize">The size, in bytes, of the chunks to read from the input stream and encrypt. Must be a positive integer. The
        /// default is AesGcmStreamCipher.DefaultChunkSize.</param>
        /// <param name="leaveInputOpen">true to leave the input stream open after the operation completes; otherwise, false.</param>
        /// <param name="leaveOutputOpen">true to leave the output stream open after the operation completes; otherwise, false.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous encryption operation.</returns>
        Task EncryptAsync(Stream input, Stream output, int chunkSize = AesGcmStreamCipher.DefaultChunkSize, bool leaveInputOpen = true, bool leaveOutputOpen = true, CancellationToken ct = default);

        /// <summary>
        /// Asynchronously decrypts data from the specified input stream and writes the decrypted data to the specified
        /// output stream.
        /// </summary>
        /// <param name="input">The stream containing the encrypted data to decrypt. Must be readable and positioned at the start of the
        /// data to decrypt.</param>
        /// <param name="output">The stream to which the decrypted data will be written. Must be writable.</param>
        /// <param name="leaveInputOpen">true to leave the input stream open after the operation completes; otherwise, false.</param>
        /// <param name="leaveOutputOpen">true to leave the output stream open after the operation completes; otherwise, false.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous decryption operation.</returns>
        Task DecryptAsync(Stream input, Stream output, bool leaveInputOpen = true, bool leaveOutputOpen = true, CancellationToken ct = default);

        /// <summary>
        /// Asynchronously encrypts the data from the specified input stream using AES-GCM and returns a stream
        /// containing the encrypted data.
        /// </summary>
        /// <remarks>The returned stream contains the encrypted data in a format compatible with AES-GCM
        /// decryption. The method does not modify the position of the input stream. The caller should ensure that the
        /// input stream remains valid for the duration of the operation.</remarks>
        /// <param name="input">The input stream containing the data to encrypt. The stream must be readable.</param>
        /// <param name="chunkSize">The size, in bytes, of the chunks to use when reading and encrypting the input stream. Must be a positive
        /// integer.</param>
        /// <param name="leaveOpen">true to leave the input stream open after the operation completes; otherwise, false.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a stream with the encrypted
        /// data. The caller is responsible for disposing the returned stream.</returns>
        Task<Stream> EncryptAsync(Stream input, int chunkSize = AesGcmStreamCipher.DefaultChunkSize, bool leaveOpen = false, CancellationToken ct = default);

        /// <summary>
        /// Asynchronously decrypts the data from the specified input stream and returns a stream containing the
        /// decrypted data.
        /// </summary>
        /// <param name="input">The input stream containing the encrypted data to decrypt. The stream must be readable.</param>
        /// <param name="leaveOpen">true to leave the input stream open after the operation completes; otherwise, false.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous decryption operation. The task result contains a stream with the
        /// decrypted data. The caller is responsible for disposing the returned stream.</returns>
        Task<Stream> DecryptAsync(Stream input, bool leaveOpen = false, CancellationToken ct = default);
    }
}

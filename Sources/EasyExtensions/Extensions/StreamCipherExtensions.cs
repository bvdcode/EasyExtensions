// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Abstractions;
using System;
using System.Text;

namespace EasyExtensions.Extensions
{
    /// <summary>
    /// Provides extension methods for encrypting plain text using an implementation of a stream cipher.
    /// </summary>
    public static class StreamCipherExtensions
    {
        /// <summary>
        /// Encrypts the specified plain text using the provided stream cipher and returns the encrypted data as a byte
        /// array.
        /// </summary>
        /// <param name="streamCipher">The stream cipher to use for encrypting the plain text. Must not be null.</param>
        /// <param name="plainText">The plain text to encrypt. Cannot be null.</param>
        /// <returns>A byte array containing the encrypted representation of the input plain text.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="plainText"/> is null.</exception>
        public static byte[] Encrypt(this IStreamCipher streamCipher, string plainText)
        {
            if (plainText == null)
            {
                throw new ArgumentNullException(nameof(plainText));
            }
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            using var inputStream = new System.IO.MemoryStream(plainBytes);
            using var outputStream = new System.IO.MemoryStream();
            streamCipher.EncryptAsync(inputStream, outputStream).GetAwaiter().GetResult();
            return outputStream.ToArray();
        }

        /// <summary>
        /// Decrypts the specified byte array using the provided stream cipher and returns the resulting plaintext as a
        /// UTF-8 encoded string.
        /// </summary>
        /// <param name="streamCipher">The stream cipher to use for decryption. Must implement the IStreamCipher interface.</param>
        /// <param name="cipherBytes">The encrypted data to decrypt, as a byte array. Cannot be null.</param>
        /// <returns>A string containing the decrypted plaintext, decoded from UTF-8.</returns>
        /// <exception cref="ArgumentNullException">Thrown if cipherBytes is null.</exception>
        public static string Decrypt(this IStreamCipher streamCipher, byte[] cipherBytes)
        {
            if (cipherBytes == null)
            {
                throw new ArgumentNullException(nameof(cipherBytes));
            }
            using var inputStream = new System.IO.MemoryStream(cipherBytes);
            using var outputStream = new System.IO.MemoryStream();
            streamCipher.DecryptAsync(inputStream, outputStream).GetAwaiter().GetResult();
            byte[] plainBytes = outputStream.ToArray();
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}

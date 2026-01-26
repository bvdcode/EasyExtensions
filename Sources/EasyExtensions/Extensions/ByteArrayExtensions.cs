// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.IO;

namespace EasyExtensions
{
    /// <summary>
    /// ByteArrayExtensions
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Calculate SHA256 hash of byte array.
        /// </summary>
        /// <param name="bytes"> Data to calculate hash. </param>
        /// <returns> SHA256 hash of byte array. </returns>
        public static string Sha256(this byte[] bytes)
        {
            using MemoryStream memoryStream = new MemoryStream(bytes);
            return memoryStream.Sha256();
        }

        /// <summary>
        /// Calculate SHA512 hash of byte array.
        /// </summary>
        /// <param name="bytes"> Data to calculate hash. </param>
        /// <returns> SHA512 hash of byte array. </returns>
        public static string Sha512(this byte[] bytes)
        {
            using MemoryStream memoryStream = new MemoryStream(bytes);
            return memoryStream.Sha512();
        }

        /// <summary>
        /// Converts the specified byte array to its equivalent lowercase hexadecimal string representation.
        /// </summary>
        /// <param name="bytes">The array of bytes to convert to a hexadecimal string. Cannot be null.</param>
        /// <returns>A string containing the lowercase hexadecimal representation of the input byte array, or an empty string if
        /// the array is empty.</returns>
        public static string ToHexStringLower(this byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return string.Empty;
            }

            char[] chars = new char[bytes.Length * 2];
            int c = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                byte b = bytes[i];
                int hi = b >> 4;
                int lo = b & 0xF;

                chars[c++] = (char)(hi < 10 ? '0' + hi : 'a' + (hi - 10));
                chars[c++] = (char)(lo < 10 ? '0' + lo : 'a' + (lo - 10));
            }

            return new string(chars);
        }
    }
}

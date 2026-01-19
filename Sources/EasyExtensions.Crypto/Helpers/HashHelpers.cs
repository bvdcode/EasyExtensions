// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace EasyExtensions.Crypto.Helpers
{
    /// <summary>
    /// Provides helper methods for validating and formatting cryptographic hash values.
    /// </summary>
    /// <remarks>This class includes utility methods for working with 256-bit hash values, such as verifying
    /// their format and converting hash data to hexadecimal string representations. All members are static and
    /// thread-safe.</remarks>
    public static partial class HashHelpers
    {
        /// <summary>
        /// Determines whether the specified string is a valid 256-bit hash value.
        /// </summary>
        /// <param name="hash">The string to validate as a 256-bit hash. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <returns>true if the input string is a valid 256-bit hash; otherwise, false.</returns>
        public static bool IsValidHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                return false;
            }
            return Hash256BitRegex().IsMatch(hash);
        }

        /// <summary>
        /// Computes the hash of the data from the specified stream and returns it as a lowercase hexadecimal string.
        /// </summary>
        /// <param name="input">The input stream containing the data to hash. The stream must be readable and positioned at the start of the
        /// data to hash.</param>
        /// <returns>A lowercase hexadecimal string representing the hash of the input data.</returns>
        [Obsolete("Use EasyExtensions.Crypto.Hashing.HashingExtensions.ComputeHashToHex extension method instead.")]
        public static string HashToHex(Stream input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] result = sha256.ComputeHash(input);
            return Convert.ToHexString(result).ToLowerInvariant();
        }

        [GeneratedRegex("^[0-9a-f]{64}$", RegexOptions.Compiled)]
        private static partial Regex Hash256BitRegex();
    }
}

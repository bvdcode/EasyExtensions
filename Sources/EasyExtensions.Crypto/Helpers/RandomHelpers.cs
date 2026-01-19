// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Security.Cryptography;

namespace EasyExtensions.Crypto.Helpers
{
    /// <summary>
    /// Provides helper methods for generating cryptographically secure random data.
    /// </summary>
    public static class RandomHelpers
    {
        /// <summary>
        /// Generates a cryptographically strong sequence of random bytes of the specified length.
        /// </summary>
        /// <remarks>This method uses a cryptographically secure random number generator suitable for
        /// creating keys, salts, or other security-sensitive data.</remarks>
        /// <param name="length">The number of random bytes to generate. Must be non-negative.</param>
        /// <returns>A byte array containing cryptographically secure random values. The length of the array is equal to the
        /// specified length. If length is zero, an empty array is returned.</returns>
        public static byte[] GetRandomBytes(int length)
        {
            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}

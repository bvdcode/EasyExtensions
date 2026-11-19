// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyExtensions.Crypto
{
    /// <summary>
    /// Key derivation using HKDF (RFC 5869) over HMAC-SHA256.
    /// Provides deterministic subkeys from a master key and context info (purpose), with optional salt.
    /// </summary>
    public static class KeyDerivation
    {
        private const int MaximumSubkeyLength = 255 * SHA256.HashSizeInBytes;

        /// <summary>
        /// HKDF (RFC 5869) over HMAC-SHA256: masterKey + info (+ optional salt) -> subkey.
        /// </summary>
        public static byte[] DeriveSubkey(
            ReadOnlySpan<byte> masterKey,
            ReadOnlySpan<byte> info,
            int lengthBytes,
            ReadOnlySpan<byte> salt = default)
        {
            if (lengthBytes == 0)
            {
                return [];
            }
            ArgumentOutOfRangeException.ThrowIfNegative(lengthBytes);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(lengthBytes, MaximumSubkeyLength);

            byte[] subkey = new byte[lengthBytes];
            HKDF.DeriveKey(HashAlgorithmName.SHA256, masterKey, subkey, salt, info);
            return subkey;
        }

        /// <summary>
        /// String-based wrapper for compatibility: masterKey + purpose -> subkey.
        /// </summary>
        public static byte[] DeriveSubkey(
            string masterKey,
            string purpose,
            int lengthBytes,
            string? salt = null)
        {
            ArgumentNullException.ThrowIfNull(masterKey);
            ArgumentNullException.ThrowIfNull(purpose);
            if (lengthBytes == 0)
            {
                return [];
            }

            byte[] masterBytes = Encoding.UTF8.GetBytes(masterKey);
            byte[] infoBytes = Encoding.UTF8.GetBytes(purpose);
            byte[]? saltBytes = salt is null ? null : Encoding.UTF8.GetBytes(salt);

            try
            {
                return DeriveSubkey(
                    masterBytes,
                    infoBytes,
                    lengthBytes,
                    saltBytes is null ? [] : saltBytes);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(masterBytes);
                CryptographicOperations.ZeroMemory(infoBytes);
                if (saltBytes is not null)
                {
                    CryptographicOperations.ZeroMemory(saltBytes);
                }
            }
        }

        /// <summary>
        /// Base64 helper.
        /// </summary>
        public static string DeriveSubkeyBase64(
            string masterKey,
            string purpose,
            int lengthBytes,
            string? salt = null)
        {
            byte[] bytes = DeriveSubkey(masterKey, purpose, lengthBytes, salt);
            return Convert.ToBase64String(bytes);
        }
    }
}

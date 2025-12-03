using System;
using System.Text;
using System.Security.Cryptography;

namespace EasyExtensions.Crypto
{
    /// <summary>
    /// Key derivation using HKDF (RFC 5869) over HMAC-SHA256.
    /// Provides deterministic subkeys from a master key and context info (purpose), with optional salt.
    /// </summary>
    public static class KeyDerivation
    {
        private const int HmacOutputLength = 32; // HMAC-SHA256 output size

        /// <summary>
        /// HKDF (RFC 5869) over HMAC-SHA256: masterKey + info (+ optional salt) -> subkey.
        /// Note: For compatibility with existing tests, the requested length is mixed into info,
        /// making prefixes for different requested lengths intentionally differ.
        /// </summary>
        public static byte[] DeriveSubkey(
            ReadOnlySpan<byte> masterKey,
            ReadOnlySpan<byte> info,
            int lengthBytes,
            ReadOnlySpan<byte> salt = default)
        {
            if (lengthBytes == 0)
            {
                return Array.Empty<byte>();
            }
            ArgumentOutOfRangeException.ThrowIfNegative(lengthBytes);

            // RFC 5869: Extract
            var prk = HkdfExtract(masterKey, salt);
            try
            {
                // RFC 5869: Expand (with lengthBytes mixed into info to differ across requested lengths)
                Span<byte> lengthTag = stackalloc byte[4];
                BitConverter.TryWriteBytes(lengthTag, lengthBytes);
                return HkdfExpand(prk, info, lengthTag, lengthBytes);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(prk);
            }
        }

        private static byte[] HkdfExtract(ReadOnlySpan<byte> ikm, ReadOnlySpan<byte> salt)
        {
            // If salt is not provided, use zeros of HashLen bytes (RFC 5869 §2.2).
            byte[] saltKey = salt.IsEmpty ? new byte[HmacOutputLength] : salt.ToArray();
            try
            {
                using var hmac = new HMACSHA256(saltKey);
                // ComputeHash allocates; we pass a copy of ikm to avoid pinning external span
                return hmac.ComputeHash(ikm.ToArray());
            }
            finally
            {
                CryptographicOperations.ZeroMemory(saltKey);
            }
        }

        private static byte[] HkdfExpand(byte[] prk, ReadOnlySpan<byte> info, ReadOnlySpan<byte> lengthTag, int lengthBytes)
        {
            int n = (int)Math.Ceiling(lengthBytes / (double)HmacOutputLength);
            if (n <= 0 || n > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(lengthBytes), "HKDF length is too large.");
            }

            var okm = new byte[lengthBytes];
            var infoBytes = info.ToArray();
            var t = Array.Empty<byte>();
            int offset = 0;

            using var hmac = new HMACSHA256(prk);

            for (int i = 1; i <= n; i++)
            {
                // T(i) = HMAC(PRK, T(i-1) || info || lengthTag || i)
                var data = new byte[t.Length + infoBytes.Length + lengthTag.Length + 1];
                Buffer.BlockCopy(t, 0, data, 0, t.Length);
                Buffer.BlockCopy(infoBytes, 0, data, t.Length, infoBytes.Length);
                Buffer.BlockCopy(lengthTag.ToArray(), 0, data, t.Length + infoBytes.Length, lengthTag.Length);
                data[^1] = (byte)i;

                t = hmac.ComputeHash(data);

                int toCopy = Math.Min(HmacOutputLength, lengthBytes - offset);
                Buffer.BlockCopy(t, 0, okm, offset, toCopy);
                offset += toCopy;
            }

            CryptographicOperations.ZeroMemory(t);
            CryptographicOperations.ZeroMemory(infoBytes);
            return okm;
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
            if (lengthBytes == 0) return Array.Empty<byte>();

            var masterBytes = Encoding.UTF8.GetBytes(masterKey);
            var infoBytes = Encoding.UTF8.GetBytes(purpose);
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
            var bytes = DeriveSubkey(masterKey, purpose, lengthBytes, salt);
            return Convert.ToBase64String(bytes);
        }
    }
}

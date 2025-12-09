using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyExtensions.Crypto
{
    /// <summary>
    /// Key derivation using HKDF (RFC 5869) over HMAC-SHA256.
    /// Provides deterministic subkeys from a master key and context info (purpose), with optional salt.
    /// Canonical implementation: Extract(PRK) then Expand: T(1)=HMAC(PRK, info || 0x01), T(i)=HMAC(PRK, T(i-1) || info || i).
    /// </summary>
    public static class KeyDerivation
    {
        private const int HmacOutputLength = 32; // HMAC-SHA256 output size

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

            var prk = HkdfExtract(masterKey, salt);
            try
            {
                return HkdfExpand(prk, info, lengthBytes);
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
                // ComputeHash allocates; pass a copy of ikm to avoid pinning external span
                return hmac.ComputeHash(ikm.ToArray());
            }
            finally
            {
                CryptographicOperations.ZeroMemory(saltKey);
            }
        }

        private static byte[] HkdfExpand(byte[] prk, ReadOnlySpan<byte> info, int lengthBytes)
        {
            int n = (int)Math.Ceiling(lengthBytes / (double)HmacOutputLength);
            if (n <= 0 || n > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(lengthBytes), "HKDF length is too large.");
            }

            var okm = new byte[lengthBytes];
            var infoBytes = info.ToArray();
            byte[] tPrev = [];
            int offset = 0;

            using var hmac = new HMACSHA256(prk);

            for (int i = 1; i <= n; i++)
            {
                // T(i) = HMAC(PRK, T(i-1) || info || i)
                var dataLen = tPrev.Length + infoBytes.Length + 1;
                var data = new byte[dataLen];
                if (tPrev.Length > 0)
                {
                    Buffer.BlockCopy(tPrev, 0, data, 0, tPrev.Length);
                }
                if (infoBytes.Length > 0)
                {
                    Buffer.BlockCopy(infoBytes, 0, data, tPrev.Length, infoBytes.Length);
                }
                data[^1] = (byte)i;

                var t = hmac.ComputeHash(data);
                int toCopy = Math.Min(HmacOutputLength, lengthBytes - offset);
                Buffer.BlockCopy(t, 0, okm, offset, toCopy);
                offset += toCopy;

                // Zero and move T(i) -> T(i-1)
                CryptographicOperations.ZeroMemory(tPrev);
                tPrev = t;
                CryptographicOperations.ZeroMemory(data);
            }

            // Cleanup
            CryptographicOperations.ZeroMemory(tPrev);
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
            if (lengthBytes == 0)
            {
                return [];
            }

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

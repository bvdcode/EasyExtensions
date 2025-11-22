using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace EasyExtensions.Crypto
{
    /// <summary>
    /// Provides methods for deriving cryptographic subkeys from a master key and a specified purpose using HMAC-SHA256.
    /// </summary>
    /// <remarks>The KeyDerivation class enables deterministic generation of subkeys for different application
    /// scenarios, such as per-user or per-purpose secrets, by combining a master key with a unique purpose string. This
    /// approach helps ensure that derived keys are unique and isolated for each intended use. All members are static
    /// and thread-safe.</remarks>
    public static class KeyDerivation
    {
        /// <summary>
        /// Derives a deterministic subkey from the specified master key and purpose using HMAC-SHA256.
        /// </summary>
        /// <remarks>This method uses HMAC-SHA256 to derive a subkey that is unique to the given purpose
        /// and master key. If the requested length exceeds 32 bytes, additional HMAC blocks are concatenated to reach
        /// the desired length. The method is suitable for generating cryptographic keys for different contexts from a
        /// single master key. The caller is responsible for ensuring that the master key and purpose are chosen
        /// appropriately for their security requirements.</remarks>
        /// <param name="masterKey">The master key as a UTF-8 encoded string. Cannot be null.</param>
        /// <param name="purpose">A string that specifies the intended purpose of the derived subkey. This value differentiates subkeys
        /// derived from the same master key. Cannot be null.</param>
        /// <param name="lengthBytes">The desired length, in bytes, of the derived subkey. Must be a positive integer.</param>
        /// <returns>A byte array containing the derived subkey of the specified length. The same inputs will always produce the
        /// same output.</returns>
        public static byte[] DeriveSubkey(string masterKey, string purpose, int lengthBytes)
        {
            // masterKey + purpose -> HMAC-SHA256 -> target length
            var masterBytes = Encoding.UTF8.GetBytes(masterKey);
            var purposeBytes = Encoding.UTF8.GetBytes(purpose);

            using var hmac = new HMACSHA256(masterBytes);
            var hash = hmac.ComputeHash(purposeBytes);

            if (lengthBytes <= hash.Length)
            {
                var result = new byte[lengthBytes];
                Array.Copy(hash, result, lengthBytes);
                return result;
            }

            // If you need more than 32 bytes, you can simply "pull" more blocks
            // with different counters. This allows for generating longer keys
            // while still being deterministic and tied to the master key and purpose.
            var buffer = new byte[lengthBytes];
            int offset = 0;
            int counter = 1;

            while (offset < lengthBytes)
            {
                var counterBytes = BitConverter.GetBytes(counter++);
                var blockInput = purposeBytes.Concat(counterBytes).ToArray();
                var block = hmac.ComputeHash(blockInput);

                int toCopy = Math.Min(block.Length, lengthBytes - offset);
                Array.Copy(block, 0, buffer, offset, toCopy);
                offset += toCopy;
            }

            return buffer;
        }

        /// <summary>
        /// Derives a subkey from the specified master key and purpose, and returns the result as a Base64-encoded
        /// string.
        /// </summary>
        /// <param name="masterKey">The master key used as the basis for subkey derivation. Cannot be null or empty.</param>
        /// <param name="purpose">A string that specifies the intended purpose of the derived subkey. Used to ensure subkeys are unique for
        /// different purposes. Cannot be null or empty.</param>
        /// <param name="lengthBytes">The desired length, in bytes, of the derived subkey. Must be a positive integer.</param>
        /// <returns>A Base64-encoded string representing the derived subkey.</returns>
        public static string DeriveSubkeyBase64(string masterKey, string purpose, int lengthBytes)
        {
            var bytes = DeriveSubkey(masterKey, purpose, lengthBytes);
            return Convert.ToBase64String(bytes);
        }
    }
}

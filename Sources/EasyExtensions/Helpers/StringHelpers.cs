using System;
using System.Security.Cryptography;

namespace EasyExtensions.Helpers
{
    /// <summary>
    /// <see cref="string"/> helpers.
    /// </summary>
    public static class StringHelpers
    {
        /// <summary>
        /// Default randomizer charset.
        /// </summary>
        public const string DefaultCharset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                      "abcdefghijklmnopqrstuvwxyz" +
                                      "0123456789";

        #region Pseudo-random generation

        /// <summary>
        /// Fast generate pseudo random string with default charset and string length.
        /// </summary>
        /// <returns> Pseudo-random string. </returns>
        public static string CreatePseudoRandomString()
        {
            const int defaultRandomStringLength = 32;
            return CreatePseudoRandomString(defaultRandomStringLength);
        }

        /// <summary>
        /// Fast generate pseudo random string with default charset and specified string length.
        /// </summary>
        /// <param name="length"> Result string length. </param>
        /// <returns> Pseudo-random string. </returns>
        public static string CreatePseudoRandomString(int length)
        {
            return CreatePseudoRandomString(length, DefaultCharset);
        }

        /// <summary>
        /// Fast generate pseudo random string with specified charset and string length.
        /// </summary>
        /// <param name="charset"> Generator charset. </param>
        /// <param name="length"> Result string length. </param>
        /// <returns> Pseudo-random string. </returns>
        public static string CreatePseudoRandomString(int length, string charset)
        {
            return GetRandomString(length, charset, true);
        }

        #endregion

        #region Really random generation

        /// <summary>
        /// Generate random string with default charset and string length.
        /// </summary>
        /// <returns> Really random string. </returns>
        public static string CreateRandomString()
        {
            return CreateRandomString(byte.MaxValue);
        }

        /// <summary>
        /// Generate random string with default charset and specified string length.
        /// </summary>
        /// <param name="length"> Result string length. </param>
        /// <returns> Really random string. </returns>
        public static string CreateRandomString(int length)
        {
            return CreateRandomString(length, DefaultCharset);
        }

        /// <summary>
        /// Generate random string with specified charset and string length.
        /// </summary>
        /// <param name="charset"> Generator charset. </param>
        /// <param name="length"> Result string length. </param>
        /// <returns> Really random string. </returns>
        public static string CreateRandomString(int length, string charset)
        {
            return GetRandomString(length, charset, true);
        }

        #endregion

        private static string GetRandomString(int length, string charset, bool useCryptoRandomNumberGenerator)
        {
            Random random = new System.Random();
            var stringChars = new char[length];
            for (int i = 0; i < stringChars.Length; i++)
            {
                int randomIndex = useCryptoRandomNumberGenerator ?
                RandomNumberGenerator.GetInt32(charset.Length) :
                random.Next(charset.Length);
                stringChars[i] = charset[randomIndex];
            }

            return new string(stringChars);
        }
    }
}
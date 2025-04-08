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
        public const string DefaultCharset =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyz" +
            "0123456789";

        /// <summary>
        /// Hide email address.
        /// </summary>
        /// <param name="email">Email address.</param>
        /// <param name="hiddenChar">Character used to hide email address.</param>
        /// <returns>Hidden email address, ex. t...a@test.com</returns>
        public static string HideEmail(string email, char hiddenChar = '*')
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return email;
            }
            var atIndex = email.IndexOf('@');
            if (atIndex < 1)
            {
                return email;
            }
            // t....a@test.com
            string firstPart = email[..atIndex];
            string lastPart = email[atIndex..];
            if (firstPart.Length < 3)
            {
                return new string(hiddenChar, firstPart.Length) + lastPart;
            }
            string hiddenPart = firstPart[1..^1];
            string hidden = firstPart[0] + new string(hiddenChar, hiddenPart.Length) + firstPart[^1];
            return hidden + lastPart;
        }   

        /// <summary>
        /// Fast generate pseudo random string with <see cref="DefaultCharset"/> and string length.
        /// </summary>
        /// <returns> Pseudo-random string. </returns>
        public static string CreatePseudoRandomString()
        {
            const int defaultRandomStringLength = 32;
            return CreatePseudoRandomString(defaultRandomStringLength);
        }

        /// <summary>
        /// Fast generate pseudo random string with <see cref="DefaultCharset"/> and specified string length.
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

        /// <summary>
        /// Generate random string with <see cref="DefaultCharset"/> and string length.
        /// </summary>
        /// <returns> Really random string. </returns>
        public static string CreateRandomString()
        {
            return CreateRandomString(byte.MaxValue);
        }

        /// <summary>
        /// Generate random string with <see cref="DefaultCharset"/> and specified string length.
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

        /// <summary>
        /// Remove spaces from string - trim, replace new lines and multiple spaces.
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static string RemoveSpaces(string? comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return string.Empty;
            }
            comment = comment
                .Replace('\n', ' ')
                .Replace("\r", " ")
                .Trim();
            while (comment.Contains("  "))
            {
                comment = comment.Replace("  ", " ");
            }
            return comment;
        }

        private static string GetRandomString(int length, string charset, bool useCryptoRandomNumberGenerator)
        {
            Random? random = useCryptoRandomNumberGenerator ? null : new Random();
            var stringChars = new char[length];
            for (int i = 0; i < stringChars.Length; i++)
            {
                int randomIndex = useCryptoRandomNumberGenerator ?
                RandomNumberGenerator.GetInt32(charset.Length) :
                random!.Next(charset.Length);
                stringChars[i] = charset[randomIndex];
            }

            return new string(stringChars);
        }
    }
}
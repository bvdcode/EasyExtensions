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
        /// Check if two strings are similar.
        /// </summary>
        /// <param name="left"> The first string. </param>
        /// <param name="right"> The second string. </param>
        /// <param name="threshold"> Similarity threshold. </param>
        public static bool IsMatch(string left, string right, double threshold = 0.8)
        {
            if (threshold < 0 || threshold > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be between 0 and 1.");
            }
            if (left.Trim() == right.Trim())
            {
                return true;
            }
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }
            int maxLength = Math.Max(left.Length, right.Length);
            int distance = LevenshteinDistance(left, right);
            double similarity = 1.0 - (double)distance / maxLength;
            return similarity >= threshold;
        }

        /// <summary>
        /// Calculate Levenshtein distance.
        /// </summary>
        /// <param name="left"> The first string. </param>
        /// <param name="right"> The second string. </param>
        /// <returns> Levenshtein distance. </returns>
        public static int LevenshteinDistance(string left, string right)
        {
            int n = left.Length;
            int m = right.Length;
            int[,] d = new int[n + 1, m + 1];
            for (int i = 0; i <= n; i++)
            {
                d[i, 0] = i;
            }
            for (int j = 0; j <= m; j++)
            {
                d[0, j] = j;
            }
            for (int j = 1; j <= m; j++)
            {
                for (int i = 1; i <= n; i++)
                {
                    int cost = left[i - 1] == right[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

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
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RemoveSpaces(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }
            content = content
                .Replace("\\n", "\n")
                .Replace('\r', '\n')
                .Replace('\n', ' ')
                .Trim();
            while (content.Contains("  "))
            {
                content = content.Replace("  ", " ");
            }
            while (content.Contains("\n\n"))
            {
                content = content.Replace("\n\n", "\n");
            }
            return content.Trim();
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
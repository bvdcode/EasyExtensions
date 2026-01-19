// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

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
            double similarity = 1.0 - ((double)distance / maxLength);
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
        /// Hide email address(es).
        /// </summary>
        /// <param name="emailString">One or more email addresses.</param>
        /// <param name="hiddenChar">Character used to hide email address.</param>
        /// <returns>Hidden email address(es), ex. t***a@test.com</returns>
        public static string HideEmail(string emailString, char hiddenChar = '*')
        {
            if (string.IsNullOrWhiteSpace(emailString))
            {
                return emailString;
            }

            // Regex pattern to match email addresses
            // This pattern looks for something@domain.com format
            string emailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";

            return Regex.Replace(emailString, emailPattern, match =>
            {
                string email = match.Value;
                int atIndex = email.IndexOf('@');

                if (atIndex < 1)
                {
                    return email; // Not a valid email or @ is the first character
                }

                string firstPart = email[..atIndex];
                string lastPart = email[atIndex..];

                if (firstPart.Length < 3)
                {
                    // If the local part is less than 3 characters, replace all with hidden char
                    return new string(hiddenChar, firstPart.Length) + lastPart;
                }
                else
                {
                    // Keep first and last character, replace middle with hidden char
                    return firstPart[0] + new string(hiddenChar, firstPart.Length - 2) + firstPart[^1] + lastPart;
                }
            });
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
        /// Remove double spaces and double new lines from content.
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Content without double spaces and double new lines.</returns>
        public static string RemoveDoubleSpaces(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }
            while (content.Contains("  "))
            {
                content = content.Replace("  ", " ");
            }
            while (content.Contains("\r\n\r\n"))
            {
                content = content.Replace("\r\n\r\n", "\r\n");
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

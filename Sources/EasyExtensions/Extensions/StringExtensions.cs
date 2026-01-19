// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Text;

namespace EasyExtensions.Extensions
{
    /// <summary>
    /// <see cref="string"/> extensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Make first letter as lower case. If text is null or whitespace - returns <see cref="string.Empty"/>
        /// </summary>
        /// <returns> Text with lower case first letter. </returns>
        public static string ToLowerFirstLetter(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }
            return char.ToLower(text[0]) + text[1..];
        }

        /// <summary>
        /// Make first letter as upper case. If text is null or whitespace - returns <see cref="string.Empty"/>
        /// </summary>
        /// <returns> Text with upper case first letter. </returns>
        public static string ToUpperFirstLetter(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }
            return char.ToUpper(text[0]) + text[1..];
        }

        /// <summary>
        /// Create SHA256 hash of specified text string.
        /// </summary>
        /// <returns> SHA256 hash. </returns>
        public static string Sha256(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }
            return Encoding.UTF8.GetBytes(str).Sha256();
        }

        /// <summary>
        /// Create SHA512 hash of specified text string.
        /// </summary>
        /// <returns> SHA512 hash. </returns>
        public static string Sha512(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }
            return Encoding.UTF8.GetBytes(str).Sha512();
        }

        /// <summary>
        /// Read only numbers from specified string.
        /// </summary>
        /// <returns> Parsed number, or -1 by default. </returns>
        public static long ReadOnlyNumbers(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return -1;
            }

            StringBuilder sb = new StringBuilder();
            foreach (char ch in str)
            {
                if (char.IsDigit(ch))
                {
                    sb.Append(ch);
                }
            }

            bool parsed = long.TryParse(sb.ToString(), out long result);
            return parsed ? result : -1;
        }
    }
}

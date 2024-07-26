using System.Text;
using System.Security.Cryptography;

namespace EasyExtensions
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
        /// Create SHA512 hash of specified text string.
        /// </summary>
        /// <returns> SHA512 hash. </returns>
        public static string SHA512(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            using SHA512 hash = System.Security.Cryptography.SHA512.Create();
            byte[] hashedInputBytes = hash.ComputeHash(bytes);
            StringBuilder hashedInputStringBuilder = new StringBuilder(128);
            foreach (byte b in hashedInputBytes)
            {
                hashedInputStringBuilder.Append(b.ToString("X2"));
            }
            return hashedInputStringBuilder.ToString().ToLower();
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
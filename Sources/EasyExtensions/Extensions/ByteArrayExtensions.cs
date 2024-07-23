using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace EasyExtensions
{
    /// <summary>
    /// ByteArrayExtensions
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Calculate SHA512 hash of byte array.
        /// </summary>
        /// <param name="bytes"> Data to calculate hash. </param>
        /// <returns> SHA512 hash of byte array. </returns>
        public static string SHA512(this byte[] bytes)
        {
            using MemoryStream memoryStream = new MemoryStream(bytes);
            return memoryStream.SHA512();
        }

        /// <summary>
        /// Calculate SHA512 hash of byte stream.
        /// </summary>
        /// <param name="stream"> Data to calculate hash. </param>
        /// <returns> SHA512 hash of byte stream. </returns>
        public static string SHA512(this Stream stream)
        {
            using SHA512 sha = System.Security.Cryptography.SHA512.Create();
            byte[] array = sha.ComputeHash(stream);
            StringBuilder stringBuilder = new StringBuilder(128);
            byte[] array2 = array;
            foreach (byte b in array2)
            {
                stringBuilder.Append(b.ToString("X2"));
            }
            return stringBuilder.ToString().ToLower();
        }
    }
}
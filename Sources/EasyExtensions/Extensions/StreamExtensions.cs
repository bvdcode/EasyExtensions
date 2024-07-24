using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EasyExtensions
{
    /// <summary>
    /// <see cref="Stream"/> extensions.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads the bytes from the current stream and writes them to the byte array.
        /// </summary>
        /// <returns> Received byte array. </returns>
        /// <exception cref="IOException"> An I/O error occurred. </exception>
        /// <exception cref="ArgumentNullException"> Destination is null. </exception>
        /// <exception cref="ObjectDisposedException"> Either the current stream or the destination stream is disposed. </exception>
        /// <exception cref="NotSupportedException"> The current stream does not support reading, or the destination stream does not support writing. </exception>
        public static byte[] ReadToEnd(this Stream stream)
        {
            using MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Asynchronously reads the bytes from the current stream and writes them to the byte array.
        /// </summary>
        /// <returns> Received byte array. </returns>
        /// <exception cref="ArgumentNullException"> Destination is null. </exception>
        /// <exception cref="ObjectDisposedException"> Either the current stream or the destination stream is disposed. </exception>
        /// <exception cref="NotSupportedException"> The current stream does not support reading, or the destination stream does not support writing. </exception>
        public static async Task<byte[]> ReadToEndAsync(this Stream stream)
        {
            using MemoryStream ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
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
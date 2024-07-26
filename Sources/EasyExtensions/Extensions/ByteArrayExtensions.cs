using System.IO;

namespace EasyExtensions.Extensions
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
    }
}
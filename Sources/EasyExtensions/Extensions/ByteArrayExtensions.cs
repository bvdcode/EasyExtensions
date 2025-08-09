using System.IO;

namespace EasyExtensions
{
    /// <summary>
    /// ByteArrayExtensions
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Calculate SHA256 hash of byte array.
        /// </summary>
        /// <param name="bytes"> Data to calculate hash. </param>
        /// <returns> SHA256 hash of byte array. </returns>
        public static string Sha256(this byte[] bytes)
        {
            using MemoryStream memoryStream = new MemoryStream(bytes);
            return memoryStream.Sha256();
        }

        /// <summary>
        /// Calculate SHA512 hash of byte array.
        /// </summary>
        /// <param name="bytes"> Data to calculate hash. </param>
        /// <returns> SHA512 hash of byte array. </returns>
        public static string Sha512(this byte[] bytes)
        {
            using MemoryStream memoryStream = new MemoryStream(bytes);
            return memoryStream.Sha512();
        }
    }
}
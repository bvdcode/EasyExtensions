using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace EasyExtensions.AspNetCore.Extensions
{
    /// <summary>
    /// <see cref="IFormFile"/> extensions.
    /// </summary>
    public static class FormFileExtensions
    {
        /// <summary>
        /// Reads the <see cref="IFormFile"/> as a byte array.
        /// </summary>
        /// <param name="file"> The <see cref="IFormFile"/>. </param>
        /// <returns> The byte array. </returns>
        public static byte[] ToBytes(this IFormFile file)
        {
            ArgumentNullException.ThrowIfNull(file);
            using MemoryStream ms = new();
            file.CopyTo(ms);
            return ms.ToArray();
        }
    }
}

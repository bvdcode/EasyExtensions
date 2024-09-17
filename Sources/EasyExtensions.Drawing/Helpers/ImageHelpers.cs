using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace EasyExtensions.Drawing.Helpers
{
    /// <summary>
    /// Image helpers.
    /// </summary>
    public static class ImageHelpers
    {
        /// <summary>
        /// Convert image to JPEG.
        /// </summary>
        public static byte[] ConvertToJpeg(byte[] image)
        {
            return ConvertToJpeg(image, out _);
        }

        /// <summary>
        /// Convert image to JPEG.
        /// </summary>
        /// <param name="image">Image to convert.</param>
        /// <param name="loadedImage">Loaded image.</param>
        /// <returns>Byte array of image.</returns>
        public static byte[] ConvertToJpeg(byte[] image, out Image<Rgba32> loadedImage)
        {
            loadedImage = Image.Load<Rgba32>(image);
            loadedImage.Mutate(x => x.AutoOrient());
            using var ms = new MemoryStream();
            loadedImage.SaveAsJpeg(ms);
            return ms.ToArray();
        }
    }
}

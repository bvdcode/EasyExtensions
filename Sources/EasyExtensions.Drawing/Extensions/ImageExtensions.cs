using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using EasyExtensions.Drawing.Helpers;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;

namespace EasyExtensions.Drawing.Extensions
{
    /// <summary>
    /// Image extensions.
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// Draw text on image.
        /// </summary>
        public static Image<Rgba32> DrawText(this Image<Rgba32> image, string text)
        {
            Font font = FontHelpers.GetAnyFont(24);
            const int offset = 5;
            PointF pointf = new(offset, offset);
            image.Mutate(x => x.DrawText(text, font, Color.Purple, pointf));
            return image;
        }

        /// <summary>
        /// Fit image to target size and copy and blur it to background.
        /// </summary>
        /// <param name="image">Target image.</param>
        /// <param name="targetWidth">Target width.</param>
        /// <param name="targetHeight">Target height.</param>
        /// <param name="gaussianBlurLevel">Gaussian blur level (optional).</param>
        /// <returns cref="Image">Image with blured background.</returns>
        public static Image FitBluredBackground(this Image image, int targetWidth, int targetHeight, float gaussianBlurLevel = 8F)
        {
            Image template = image.Clone(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Crop,
                Size = new Size(targetWidth, targetHeight)
            }));
            template.Mutate(x => x.GaussianBlur(gaussianBlurLevel));
            using var cloned = image.Clone(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(targetWidth, targetHeight)
            }));

            int x = template.Width > cloned.Width ? (template.Width - cloned.Width) / 2 : 0;
            int y = template.Height > cloned.Height ? (template.Height - cloned.Height) / 2 : 0;

            Point point = new(x, y);
            template.Mutate(x => x.DrawImage(cloned, point, 1F));
            return template;
        }

        /// <summary>
        /// Save image as JPEG to byte array.
        /// </summary>
        /// <param name="image">Target image.</param>
        /// <returns>Byte array of image.</returns>
        public static byte[] SaveAsJpegToArray(this Image image)
        {
            using var stream = new MemoryStream();
            image.SaveAsJpeg(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Set image brightness automatically.
        /// </summary>
        public static void ApplyAutoLight(this Image<Rgba32> image)
        {
            IEnumerable<Rgba32> colors = ToColors(image);
            double average = CalculateAverageBrightness(colors);
            double brightnessRate = 100 / average;
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    var pixel = image[i, j];
                    int R = (int)(pixel.R * brightnessRate);
                    int G = (int)(pixel.G * brightnessRate);
                    int B = (int)(pixel.B * brightnessRate);
                    if (R > 255)
                    {
                        R = 255;
                    }
                    if (G > 255)
                    {
                        G = 255;
                    }
                    if (B > 255)
                    {
                        B = 255;
                    }

                    Rgba32 newPixel = new((byte)R, (byte)G, (byte)B, pixel.A);
                    image[i, j] = newPixel;
                }
            }
        }

        private static List<Rgba32> ToColors(Image<Rgba32> img)
        {
            List<Rgba32> currentColors = new();
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    currentColors.Add(img[i, j]);
                }
            }
            return currentColors;
        }

        private static double GetBrightness(Rgba32 pixel)
        {
            const double redMultiplier = 0.2126;
            const double greenMultiplier = 0.7152;
            const double blueMultiplier = 0.0722;
            double result = redMultiplier * pixel.R + greenMultiplier * pixel.G + blueMultiplier * pixel.B;
            return result;
        }

        private static double CalculateAverageBrightness(IEnumerable<Rgba32> pixels)
        {
            int count = 0;
            double sumBrightness = 0;
            foreach (var pixel in pixels)
            {
                count++;
                sumBrightness += GetBrightness(pixel);
            }
            return sumBrightness / count;
        }
    }
}
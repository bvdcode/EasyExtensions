using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace EasyExtensions.Drawing.Extensions
{
    /// <summary>
    /// Image extensions.
    /// </summary>
    public static class ImageExtensions
    {
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
    }
}
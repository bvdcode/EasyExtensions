using System.IO;
using System.Linq;
using SixLabors.Fonts;
using System.Globalization;

namespace EasyExtensions.Drawing.Helpers
{
    /// <summary>
    /// Font helpers.
    /// </summary>
    public static class FontHelpers
    {
        /// <summary>
        /// Get any font from system or get default library font.
        /// </summary>
        /// <param name="size">Font size, default is 12.</param>
        /// <returns>Font.</returns>
        public static Font GetAnyFont(float size = 12)
        {
            var found = SystemFonts.Collection
                .GetByCulture(CultureInfo.InvariantCulture)
                .FirstOrDefault();
            if (found != default)
            {
                return found.CreateFont(size);
            }
            FontCollection collection = new();
            using Stream fontStream = new MemoryStream(Resources.Fonts.Consola);
            return collection.Add(fontStream).CreateFont(size);
        }
    }
}

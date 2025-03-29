using System.IO;
using System.Linq;
using SixLabors.Fonts;
using System.Globalization;
using EasyExtensions.Fonts.Resources;

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
        /// <returns>Created Font instance</returns>
        public static Font GetAnyFont(float size = 12)
        {
            var found = SystemFonts.Collection
                .GetByCulture(CultureInfo.InvariantCulture)
                .FirstOrDefault();
            if (found != default)
            {
                return found.CreateFont(size);
            }
            return CreateFont(StaticFontName.Consola, size);
        }

        /// <summary>
        /// Create a font from byte array.
        /// </summary>
        public static Font CreateFont(StaticFontName fontName, float size = 12)
        {
            FontCollection collection = new();
            byte[] fontBytes = StaticFonts.GetFontBytes(fontName);
            using Stream fontStream = new MemoryStream(fontBytes);
            return collection.Add(fontStream).CreateFont(size);
        }
    }
}

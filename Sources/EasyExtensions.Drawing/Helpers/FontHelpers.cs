using EasyExtensions.Drawing.Properties;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        /// <param name="size"></param>
        /// <returns></returns>
        public static Font GetAnyFont(float size)
        {
            var found = SystemFonts.Collection
                .GetByCulture(CultureInfo.InvariantCulture)
                .FirstOrDefault();
            if (found != default)
            {
                return found.CreateFont(size);
            }
            FontCollection collection = new();
            using Stream fontStream = new MemoryStream(Resources.Consola);
            return collection.Add(fontStream).CreateFont(size);
        }
    }
}

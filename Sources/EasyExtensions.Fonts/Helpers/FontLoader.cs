using System;
using System.IO;
using System.Linq;
using System.Reflection;
using EasyExtensions.Fonts.Resources;

namespace EasyExtensions.Fonts.Helpers
{
    /// <summary>
    /// Provides methods for loading embedded fonts.
    /// </summary>
    internal static class FontLoader
    {
        /// <summary>
        /// Loads a font from embedded resources.
        /// </summary>
        /// <param name="fontName">The name of the font file including extension.</param>
        /// <returns>Font file as byte array.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the font file is not found.</exception>
        internal static byte[] LoadFont(string fontName)
        {
            var assembly = typeof(FontLoader).Assembly;            
            using var stream = assembly.GetManifestResourceStream(typeof(StaticFonts), fontName)
                ?? throw new FileNotFoundException($"Font file '{fontName}' not found in embedded resources.");
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}

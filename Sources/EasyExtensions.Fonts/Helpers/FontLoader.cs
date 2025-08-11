using System.IO;
using System.Collections.Concurrent;
using EasyExtensions.Fonts.Resources;

namespace EasyExtensions.Fonts.Helpers
{
    /// <summary>
    /// Provides methods for loading embedded fonts using concurrent dictionary cache.
    /// </summary>
    internal static class FontLoader
    {
        private static readonly ConcurrentDictionary<string, byte[]> _fontCache = new ConcurrentDictionary<string, byte[]>();

        /// <summary>
        /// Loads a font from embedded resources or returns it from cache.
        /// </summary>
        /// <param name="fontName">The name of the font file including extension.</param>
        /// <returns>Font file as byte array.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the font file is not found.</exception>
        internal static byte[] LoadFont(string fontName)
        {
            if (_fontCache.TryGetValue(fontName, out var fontBytes))
            {
                return fontBytes;
            }
            var assembly = typeof(FontLoader).Assembly;            
            using var stream = assembly.GetManifestResourceStream(typeof(StaticFonts), fontName)
                ?? throw new FileNotFoundException($"Font file '{fontName}' not found in embedded resources.");
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            fontBytes = memoryStream.ToArray();
            _fontCache.TryAdd(fontName, fontBytes);
            return fontBytes;
        }
    }
}

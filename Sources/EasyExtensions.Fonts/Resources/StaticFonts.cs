// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Fonts.Helpers;
using System;

namespace EasyExtensions.Fonts.Resources
{
    /// <summary>
    /// Static class to get fonts in byte array.
    /// </summary>
    public static class StaticFonts
    {
        /// <summary>
        /// Get font by name specified in <see cref="StaticFontName"/>.
        /// </summary>
        /// <param name="fontName">The font name.</param>
        /// <returns>The font in byte array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">When font name is not found.</exception>
        public static byte[] GetFontBytes(StaticFontName fontName)
        {
            string filename = fontName.ToString() + ".ttf";
            return FontLoader.LoadFont(filename);
        }
    }
}

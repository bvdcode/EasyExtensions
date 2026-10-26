// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Drawing.Helpers;
using EasyExtensions.Fonts.Resources;
using SixLabors.Fonts;

namespace EasyExtensions.Tests
{
    public class DrawingFontTests
    {
        [TestCase(StaticFontName.Tuffy, false, false)]
        [TestCase(StaticFontName.Y224, false, false)]
        [TestCase(StaticFontName.OSerif, false, false)]
        [TestCase(StaticFontName.PublicPixel, false, false)]
        [TestCase(StaticFontName.TuffyBold, true, false)]
        [TestCase(StaticFontName.TuffyItalic, false, true)]
        [TestCase(StaticFontName.TuffyBoldItalic, true, true)]
        public void CreateFont_LoadsExpectedStyle(StaticFontName fontName, bool isBold, bool isItalic)
        {
            Font result = FontHelpers.CreateFont(fontName);

            Assert.That(result.IsBold, Is.EqualTo(isBold));
            Assert.That(result.IsItalic, Is.EqualTo(isItalic));
        }
    }
}

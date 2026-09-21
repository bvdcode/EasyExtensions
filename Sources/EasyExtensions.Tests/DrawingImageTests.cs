// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Drawing.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EasyExtensions.Tests
{
    public class DrawingImageTests
    {
        [Test]
        public void DrawText_RendersIntoTheOriginalImage()
        {
            Rgba32 background = Color.White.ToPixel<Rgba32>();
            using Image<Rgba32> image = new(200, 80, background);
            Image<Rgba32> result = image.DrawText("Sample");
            Rgba32[] pixels = new Rgba32[image.Width * image.Height];
            image.CopyPixelDataTo(pixels);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.SameAs(image));
                Assert.That(pixels, Has.Some.Not.EqualTo(background));
                Assert.That(image[0, 0], Is.EqualTo(background));
            }
        }

        [Test]
        public void FitBluredBackground_PreservesTargetSizeThroughJpegEncoding()
        {
            using Image<Rgba32> source = new(40, 20, Color.Purple.ToPixel<Rgba32>());
            using Image fitted = source.FitBluredBackground(80, 80);
            using Image decoded = Image.Load(fitted.SaveAsJpegToArray());
            Assert.That(decoded.Size, Is.EqualTo(new Size(80, 80)));
        }
    }
}

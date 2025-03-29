using EasyExtensions.Drawing.Helpers;
using EasyExtensions.Fonts.Resources;

namespace EasyExtensions.Tests
{
    public class DrawingFontTests
    {
        [Test]
        public void GetAnyFont_ValidInput_ValidOutput()
        {
            var allFonts = Enum.GetValues<StaticFontName>();
            foreach (var font in allFonts)
            {
                var result = FontHelpers.CreateFont(font);
                Assert.That(result, Is.Not.Null);
            }
        }
    }
}

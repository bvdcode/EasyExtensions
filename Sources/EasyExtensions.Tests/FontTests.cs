using EasyExtensions.Fonts.Resources;

namespace EasyExtensions.Tests
{
    public class FontTests
    {
        [Test]
        public void GetAnyFont_ValidInput_ValidOutput()
        {
            var allFonts = Enum.GetValues<StaticFontName>();
            foreach (var font in allFonts)
            {
                byte[] fontBytes = StaticFonts.GetFont(font);
                Assert.That(fontBytes, Is.Not.Null);
                Assert.That(fontBytes, Is.Not.Empty);
            }
        }

        [Test]
        public void AllFonts_AreDifferent()
        {
            var allFonts = Enum.GetValues<StaticFontName>();
            var fontBytesList = allFonts.Select(StaticFonts.GetFont).ToList();

            for (int i = 0; i < fontBytesList.Count; i++)
            {
                for (int j = i + 1; j < fontBytesList.Count; j++)
                {
                    Assert.That(fontBytesList[i], Is.Not.EqualTo(fontBytesList[j]));
                }
            }
        }
    }
}

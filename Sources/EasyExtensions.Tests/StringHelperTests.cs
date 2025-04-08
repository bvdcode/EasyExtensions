using EasyExtensions.Helpers;

namespace EasyExtensions.Tests
{
    internal class StringHelperTests
    {
        [Test]
        public void HideEmail_ValidInput_ValidOutput()
        {
            const string actual = "abcabc@gmail.com";
            const string expected = "a****c@gmail.com";
            Assert.That(StringHelpers.HideEmail(actual), Is.EqualTo(expected));
        }

        [Test]
        public void HideEmail_InvalidInput_ValidOutput()
        {
            const string actual = "ac@gmail.com";
            const string expected = "**@gmail.com";
            Assert.That(StringHelpers.HideEmail(actual), Is.EqualTo(expected));
        }
    }
}

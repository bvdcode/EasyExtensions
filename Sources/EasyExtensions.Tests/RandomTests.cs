using EasyExtensions.Helpers;

namespace EasyExtensions.Tests
{
    public class RandomTests
    {
        private const int repeats = 100_000;

        [Test]
        public void PseudoRandomString_ValidInput_ValidOutput()
        {
            HashSet<string> previousRandomGenerated = [];
            for (int i = 0; i < repeats; i++)
            {
                string random = StringHelpers.CreatePseudoRandomString(16);
                bool exists = previousRandomGenerated.Contains(random);
                Assert.That(exists, Is.EqualTo(false));
                previousRandomGenerated.Add(random);
            }
        }

        [Test]
        public void RandomString_ValidInput_ValidOutput()
        {
            HashSet<string> previousRandomGenerated = [];
            for (int i = 0; i < repeats; i++)
            {
                string random = StringHelpers.CreateRandomString(12);
                bool exists = previousRandomGenerated.Contains(random);
                Assert.That(exists, Is.EqualTo(false));
                previousRandomGenerated.Add(random);
            }
        }
    }
}
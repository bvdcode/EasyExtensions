// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Helpers;

namespace EasyExtensions.Tests
{
    internal class StringHelperTests
    {
        [Test]
        public void IsMatch_ValidSameInput_ValidOutput()
        {
            const string left = "abc";
            const string right = left;
            Assert.That(StringHelpers.IsMatch(left, right, threshold: 0.8), Is.True);
        }

        [Test]
        public void IsMatch_ValidDiffInput_ValidOutput()
        {
            const string left = "abcdefghij";
            const string right = "abcdefgh12";
            Assert.That(StringHelpers.IsMatch(left, right, threshold: 0.8), Is.True);
        }

        [Test]
        public void IsMatch_ValidInput_MustBeFalse()
        {
            const string left = "abcdefghij";
            const string right = "abcdefg123";
            Assert.That(StringHelpers.IsMatch(left, right, threshold: 0.8), Is.False);
        }


        [Test]
        public void IsMatch_ValidInputZeroThresholdSameStrings_MustBeTrue()
        {
            const string left = "abcdefghij";
            const string right = left;
            Assert.That(StringHelpers.IsMatch(left, right, threshold: 0), Is.True);
        }


        [Test]
        public void IsMatch_ValidInputZeroThresholdDiffStrings_MustBeTrue()
        {
            const string left = "abcdefghij";
            const string right = left;
            Assert.That(StringHelpers.IsMatch(left, right, threshold: 0), Is.True);
        }

        [Test]
        public void IsMatch_ValidInputFullThresholdSameStrings_MustBeTrue()
        {
            const string left = "abcdefghij";
            const string right = left;
            Assert.That(StringHelpers.IsMatch(left, right, threshold: 1), Is.True);
        }

        [Test]
        public void IsMatch_ValidInputFullThresholdDiffStrings_MustBeFalse()
        {
            const string left = "vxjQVRmbc8KOTvi6vsAmi7BZjhYXilPuvxjQVRmbc8KOTvi6vsAmi7BZjhYXilPu";
            const string right = "vxjQVRmbc8KOTvi6vsAmi7BZjhYXilPuvxjQVRmbc8KOTvi6vsAmi7BZjhYXilP1";
            Assert.That(StringHelpers.IsMatch(left, right, threshold: 1), Is.False);
        }

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

        [Test]
        public void HideEmail_MultipleInput_ValidOutput()
        {
            const string actual = "ac@gmail.com <test@outlook.com>";
            const string expected = "**@gmail.com <t**t@outlook.com>";
            Assert.That(StringHelpers.HideEmail(actual), Is.EqualTo(expected));
        }
    }
}

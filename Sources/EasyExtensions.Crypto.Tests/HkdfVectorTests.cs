// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Security.Cryptography;
using System.Text;

namespace EasyExtensions.Crypto.Tests
{
    public class HkdfVectorTests
    {
        [TestCase("000102030405060708090a0b0c", "f0f1f2f3f4f5f6f7f8f9",
            "3cb25f25faacd57a90434f64d0362f2a2d2d0a90cf1a5a4c5db02d56ecc4c5bf34007208d5b887185865")]
        [TestCase("", "",
            "8da4e775a563c18f715f802a063c5a31b8a11f5c5ee1879ec3454e5f3c738d2d9d201395faa4b61a96c8")]
        public void DeriveSubkey_MatchesRfc5869Sha256Vectors(string saltHex, string infoHex, string expectedHex)
        {
            byte[] masterKey = Enumerable.Repeat((byte)0x0b, 22).ToArray();
            byte[] salt = Convert.FromHexString(saltHex);
            byte[] info = Convert.FromHexString(infoHex);
            byte[] expected = Convert.FromHexString(expectedHex);

            byte[] actual = KeyDerivation.DeriveSubkey(masterKey, info, expected.Length, salt);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DeriveSubkey_MatchesRfc5869LongInputVector()
        {
            byte[] masterKey = Enumerable.Range(0, 80).Select(value => (byte)value).ToArray();
            byte[] salt = Enumerable.Range(0x60, 80).Select(value => (byte)value).ToArray();
            byte[] info = Enumerable.Range(0xb0, 80).Select(value => (byte)value).ToArray();
            byte[] expected = Convert.FromHexString(
                "b11e398dc80327a1c8e7f78c596a49344f012eda2d4efad8a050cc4c19afa97c"
                + "59045a99cac7827271cb41c65e590e09da3275600c2f09b8367793a9aca3db71"
                + "cc30c58179ec3e87c14c01d5c1f3434f1d87");

            byte[] actual = KeyDerivation.DeriveSubkey(masterKey, info, expected.Length, salt);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(-1)]
        [TestCase(8161)]
        [TestCase(int.MaxValue)]
        public void DeriveSubkey_RejectsInvalidLengthBeforeAllocating(int length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => KeyDerivation.DeriveSubkey([1], [2], length));
        }

        [Test]
        public void DeriveSubkey_AcceptsMaximumLength()
        {
            const int length = 255 * SHA256.HashSizeInBytes;
            byte[] masterKey = [1, 2, 3];
            byte[] info = [4, 5, 6];
            byte[] salt = [7, 8, 9];

            byte[] actual = KeyDerivation.DeriveSubkey(masterKey, info, length, salt);

            Assert.That(actual, Has.Length.EqualTo(length));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("соль 🔑")]
        public void DeriveSubkey_StringOverloadPreservesUtf8Inputs(string? salt)
        {
            const string masterKey = "ключ 🔑";
            const string purpose = "шифрование";
            byte[] expected = KeyDerivation.DeriveSubkey(
                Encoding.UTF8.GetBytes(masterKey), Encoding.UTF8.GetBytes(purpose), 48,
                salt is null ? [] : Encoding.UTF8.GetBytes(salt));

            byte[] actual = KeyDerivation.DeriveSubkey(masterKey, purpose, 48, salt);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}

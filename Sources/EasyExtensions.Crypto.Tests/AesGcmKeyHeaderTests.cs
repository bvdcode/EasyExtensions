// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using EasyExtensions.Crypto.Models;

namespace EasyExtensions.Crypto.Tests
{
    [Category("Format")]
    public class AesGcmKeyHeaderTests
    {
        [Test]
        public void SerializeDeserialize_RoundTrip_WithCustomSizes()
        {
            // file header round-trip (nonceSize=3, tagSize=16, keySize=32)
            AesGcmKeyHeader expectedHeader = new(123, [1, 2, 3],
                [4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19],
                [7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38],
                12345);
            ReadOnlyMemory<byte> headerBytes = expectedHeader.ToBytes();
            using MemoryStream ms = new(headerBytes.ToArray());
            AesGcmKeyHeader parsedHeader = AesGcmKeyHeader.FromStream(ms, 3, 16);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(parsedHeader.KeyId, Is.EqualTo(expectedHeader.KeyId));
                Assert.That(parsedHeader.Nonce, Is.EqualTo(expectedHeader.Nonce));
                Assert.That(parsedHeader.Tag, Is.EqualTo(expectedHeader.Tag));
                Assert.That(parsedHeader.EncryptedKey, Is.EqualTo(expectedHeader.EncryptedKey));
                Assert.That(parsedHeader.DataLength, Is.EqualTo(expectedHeader.DataLength));
            }
        }
    }
}

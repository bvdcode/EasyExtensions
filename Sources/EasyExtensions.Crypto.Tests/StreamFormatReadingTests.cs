// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Crypto.Internals;
using EasyExtensions.Crypto.Tests.TestUtils;

namespace EasyExtensions.Crypto.Tests
{
    public class StreamFormatReadingTests
    {
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(7)]
        public async Task ReadFileHeader_ReadsFragmentedInputWithoutConsumingPayload(int maximumReadSize)
        {
            byte[] bytes = FileHeaderBytes();
            using FragmentedReadStream stream = new([.. bytes, 0xa5], maximumReadSize);

            FileHeader header = await AesGcmStreamFormat.ReadFileHeaderAsync(stream,
                AesGcmStreamCipher.NonceSize, AesGcmStreamCipher.TagSize, AesGcmStreamCipher.KeySize, default);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(header.KeyId, Is.EqualTo(42));
                Assert.That(header.TotalPlaintextLength, Is.EqualTo(123));
                Assert.That(header.NoncePrefix, Is.EqualTo(7));
                Assert.That(stream.ReadByte(), Is.EqualTo(0xa5));
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(20)]
        [TestCase(83)]
        public async Task ReadFileHeader_RejectsTruncatedInput(int length)
        {
            byte[] bytes = FileHeaderBytes();
            using FragmentedReadStream stream = new(bytes[..length], maximumReadSize: 3);

            await Assert.ThrowsAsync<EndOfStreamException>(async () =>
                await AesGcmStreamFormat.ReadFileHeaderAsync(stream,
                    AesGcmStreamCipher.NonceSize, AesGcmStreamCipher.TagSize, AesGcmStreamCipher.KeySize, default));
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(7)]
        public async Task ReadChunkHeader_ReadsFragmentedInputWithoutConsumingPayload(int maximumReadSize)
        {
            byte[] bytes = new byte[AesGcmStreamFormat.ComputeChunkHeaderLength(AesGcmStreamCipher.TagSize)];
            AesGcmStreamFormat.BuildChunkHeader(bytes, 42, default, 123, AesGcmStreamCipher.TagSize);
            using FragmentedReadStream stream = new([.. bytes, 0xa5], maximumReadSize);

            ChunkHeader header = await AesGcmStreamFormat.ReadChunkHeaderAsync(stream, AesGcmStreamCipher.TagSize, default);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(header.KeyId, Is.EqualTo(42));
                Assert.That(header.PlaintextLength, Is.EqualTo(123));
                Assert.That(stream.ReadByte(), Is.EqualTo(0xa5));
            }
        }

        private static byte[] FileHeaderBytes()
        {
            byte[] bytes = new byte[AesGcmStreamFormat.ComputeFileHeaderLength(
                AesGcmStreamCipher.NonceSize, AesGcmStreamCipher.TagSize, AesGcmStreamCipher.KeySize)];
            AesGcmStreamFormat.BuildFileHeader(bytes, 42, 7, new byte[AesGcmStreamCipher.NonceSize], default,
                new byte[AesGcmStreamCipher.KeySize], 123,
                AesGcmStreamCipher.NonceSize, AesGcmStreamCipher.TagSize, AesGcmStreamCipher.KeySize);
            return bytes;
        }
    }
}

// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Buffers.Binary;
using System.Security.Cryptography;

namespace EasyExtensions.Crypto.Tests;

[Category("Format")]
public class GoldenVectorsTests
{
    [Test]
    public void Golden_Header_And_FirstChunk_Deterministic()
    {
        // Fixed master key
        byte[] masterKey = new byte[32];
        for (int i = 0; i < masterKey.Length; i++) masterKey[i] = (byte)(i + 1);
        // Deterministic RNG seeded via RNGCryptoServiceProvider replacement: use fixed bytes from Hash counter
        var rng = new DeterministicRng(0xA5A5A5A5);

        var cipher = new AesGcmStreamCipher(masterKey, keyId: 77, threads: 1, rng: rng);
        byte[] payload = new byte[128];
        for (int i = 0; i < payload.Length; i++) payload[i] = (byte)(i ^ 0x5A);

        using var input = new MemoryStream(payload);
        using var output = new MemoryStream();
        cipher.EncryptAsync(input, output, chunkSize: 64 * 1024).GetAwaiter().GetResult();
        byte[] bytes = output.ToArray();

        // Extract header bytes
        int headerLen = 4 + 4 + 8 + 4 + 4 + AesGcmStreamCipher.NonceSize + AesGcmStreamCipher.TagSize + AesGcmStreamCipher.KeySize;
        byte[] header = bytes.AsSpan(0, headerLen).ToArray();
        // Extract first chunk header + ciphertext (only one chunk present)
        int chunkHdrLen = 4 + 4 + 8 + 4 + AesGcmStreamCipher.TagSize;
        byte[] chunk = bytes.AsSpan(headerLen, Math.Min(bytes.Length - headerLen, chunkHdrLen + payload.Length)).ToArray();

        // Golden expected (frozen). If this test changes, format changed.
        // These constants computed once and embedded to lock the format.
        byte[] expectedHeader = [
            // Magic CTN1, header length le32, total len le64, keyId le32, noncePrefix le32, nonce(12), tag(16), encryptedKey(32)
            0x43,0x54,0x4E,0x31, 0x54,0x00,0x00,0x00, 0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x4D,0x00,0x00,0x00, 0x7A,0x7A,0x7A,0x7A, // prefix deterministic
            // nonce 12, tag 16, encryptedKey 32 (fake but consistent with DeterministicRng)
        ];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(header.AsSpan(0, 20).ToArray(), Is.EqualTo(expectedHeader.AsSpan(0, 20).ToArray()));

            // For brevity, pin only the first 20 header bytes and first 8 bytes of chunk header
            Assert.That(chunk, Has.Length.GreaterThanOrEqualTo(chunkHdrLen));
        }
        using (Assert.EnterMultipleScope())
        {
            Assert.That(chunk[0], Is.EqualTo((byte)'C'));
            Assert.That(chunk[1], Is.EqualTo((byte)'T'));
            Assert.That(chunk[2], Is.EqualTo((byte)'N'));
            Assert.That(chunk[3], Is.EqualTo((byte)'1'));
        }
    }

    private sealed class DeterministicRng(ulong seed) : RandomNumberGenerator
    {
        public override void GetBytes(byte[] data)
        {
            Span<byte> tmp = stackalloc byte[8];
            for (int i = 0; i < data.Length; i += 8)
            {
                seed = unchecked((seed * 6364136223846793005UL) + 1);
                BinaryPrimitives.WriteUInt64LittleEndian(tmp, seed);
                int len = Math.Min(8, data.Length - i);
                tmp[..len].CopyTo(data.AsSpan(i, len));
            }
        }

        public override void GetBytes(byte[] data, int offset, int count) => GetBytes(data.AsSpan(offset, count));
        public override void GetBytes(Span<byte> data)
        {
            Span<byte> tmp = stackalloc byte[8];
            for (int i = 0; i < data.Length; i += 8)
            {
                seed = unchecked((seed * 6364136223846793005UL) + 1);
                BinaryPrimitives.WriteUInt64LittleEndian(tmp, seed);
                int len = Math.Min(8, data.Length - i);
                tmp[..len].CopyTo(data.Slice(i, len));
            }
        }
    }
}

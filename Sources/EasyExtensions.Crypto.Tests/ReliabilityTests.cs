// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Crypto.Models;
using EasyExtensions.Crypto.Tests.TestUtils;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace EasyExtensions.Crypto.Tests;

[Category("Reliability")]
public class ReliabilityTests
{
    private static byte[] Key() => [.. Enumerable.Range(0, 32).Select(i => (byte)i)];

    // 10. Non-seekable empty file
    [Test]
    public void NonSeekable_EmptyFile_RoundTrip_NoChunks()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 10, threads: 1);
        using var input = new MemoryStream([]);
        using var nonSeek = new NonSeekableReadStream(input);
        using var outEnc = new MemoryStream();
        cipher.EncryptAsync(nonSeek, outEnc, chunkSize: AesGcmStreamCipher.MinChunkSize).GetAwaiter().GetResult();
        outEnc.Position = 0;
        var hdr = AesGcmKeyHeader.FromStream(outEnc, AesGcmStreamCipher.NonceSize, AesGcmStreamCipher.TagSize);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(hdr.DataLength, Is.Zero);
            // There should be no more data
            Assert.That(outEnc.Position, Is.EqualTo(outEnc.Length));
        }
        // Decrypt back and verify 0 bytes
        outEnc.Position = 0;
        using var outDec = new MemoryStream();
        cipher.DecryptAsync(outEnc, outDec).GetAwaiter().GetResult();
        Assert.That(outDec.Length, Is.Zero);
    }

    // 11. ChunkSize boundaries
    [Test]
    public void ChunkSize_Boundaries_Min_And_Max()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 11, threads: 2);
        int min = AesGcmStreamCipher.MinChunkSize;
        int max = AesGcmStreamCipher.MaxChunkSize;
        byte[] data = new byte[(max * 2) + (min / 2) + 123]; // ensure partial last chunk
        for (int i = 0; i < data.Length; i++) data[i] = (byte)(i & 0xFF);
        // Test with MinChunkSize
        using var input1 = new MemoryStream(data);
        using var enc1 = new MemoryStream();
        cipher.EncryptAsync(input1, enc1, chunkSize: min).GetAwaiter().GetResult();
        var (chunksMin, lengthsMin) = ParseChunks(enc1.ToArray(), out _);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(chunksMin, Is.EqualTo((data.Length / min) + ((data.Length % min == 0) ? 0 : 1)));
            Assert.That(lengthsMin.Last(), Is.EqualTo(data.Length % min == 0 ? min : data.Length % min));
        }

        // Test with MaxChunkSize
        using var input2 = new MemoryStream(data);
        using var enc2 = new MemoryStream();
        cipher.EncryptAsync(input2, enc2, chunkSize: max).GetAwaiter().GetResult();
        var (chunksMax, lengthsMax) = ParseChunks(enc2.ToArray(), out _);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(chunksMax, Is.EqualTo((data.Length / max) + ((data.Length % max == 0) ? 0 : 1)));
            Assert.That(lengthsMax.Last(), Is.EqualTo(data.Length % max == 0 ? max : data.Length % max));
        }
    }

    // 12. Wrong magic in chunk-header
    [Test]
    public void Decrypt_WrongMagic_FailsFast()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 12, threads: 1);
        byte[] data = [.. Enumerable.Range(0, AesGcmStreamCipher.MinChunkSize * 2).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        using var enc = new MemoryStream();
        cipher.EncryptAsync(input, enc, chunkSize: AesGcmStreamCipher.MinChunkSize).GetAwaiter().GetResult();
        byte[] bytes = enc.ToArray();
        // Skip file header
        int fileHeaderLen = 4 + 4 + 8 + 4 + 4 + AesGcmStreamCipher.NonceSize + AesGcmStreamCipher.TagSize + AesGcmStreamCipher.KeySize;
        // Corrupt first 4 bytes of chunk magic
        bytes[fileHeaderLen + 0] ^= 0xFF;
        bytes[fileHeaderLen + 1] ^= 0xFF;
        bytes[fileHeaderLen + 2] ^= 0xFF;
        bytes[fileHeaderLen + 3] ^= 0xFF;
        using var tampered = new MemoryStream(bytes, writable: false);
        using var outDec = new MemoryStream();
        Assert.ThrowsAsync<InvalidDataException>(async () => await cipher.DecryptAsync(tampered, outDec));
    }

    // 13. StrictLengthCheck
    [Test]
    public void StrictLengthCheck_HeaderMismatch()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 13, threads: 1, strictLengthCheck: true);
        int L = AesGcmStreamCipher.MinChunkSize + 123;
        byte[] data = new byte[L];
        for (int i = 0; i < data.Length; i++) data[i] = (byte)(i & 0x7F);
        using var input = new MemoryStream(data);
        using var enc = new MemoryStream();
        cipher.EncryptAsync(input, enc, chunkSize: AesGcmStreamCipher.MinChunkSize).GetAwaiter().GetResult();
        byte[] bytes = enc.ToArray();
        // Modify header's total length (8 bytes at offset 8)
        void SetTotal(long val)
        {
            BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(8, 8), val);
        }
        // L-1
        SetTotal(L - 1);
        using var tamperedMinus = new MemoryStream(bytes, writable: false);
        using var outDec1 = new MemoryStream();
        Assert.ThrowsAsync<AuthenticationTagMismatchException>(async () => await cipher.DecryptAsync(tamperedMinus, outDec1));
        // L+1
        SetTotal(L + 1);
        using var tamperedPlus = new MemoryStream(bytes, writable: false);
        using var outDec2 = new MemoryStream();
        Assert.ThrowsAsync<AuthenticationTagMismatchException>(async () => await cipher.DecryptAsync(tamperedPlus, outDec2));
        // Exactly L passes
        SetTotal(L);
        using var ok = new MemoryStream(bytes, writable: false);
        using var outDecOk = new MemoryStream();
        cipher.DecryptAsync(ok, outDecOk).GetAwaiter().GetResult();
        Assert.That(outDecOk.Length, Is.EqualTo(L));
    }

    // 14. Threads guard
    [Test]
    public void Threads_Guard_Validates()
    {
        var key = Key();
        Assert.Throws<ArgumentOutOfRangeException>(() => new AesGcmStreamCipher(key, keyId: 14, threads: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AesGcmStreamCipher(key, keyId: 14, threads: 10_000));
    }

    // 15. Secure pooling proof
    [Test]
    public void SecurePooling_DoesNotLeakPlaintextPattern()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 15, threads: 2);
        byte[] pattern = new byte[AesGcmStreamCipher.MinChunkSize];
        for (int i = 0; i < pattern.Length; i++) pattern[i] = 0xAA;
        using var input = new MemoryStream(pattern);
        using var outEnc = new MemoryStream();
        cipher.EncryptAsync(input, outEnc, chunkSize: AesGcmStreamCipher.MinChunkSize).GetAwaiter().GetResult();
        outEnc.Position = 0;
        using var outDec = new MemoryStream();
        cipher.DecryptAsync(outEnc, outDec).GetAwaiter().GetResult();
        // Immediately rent a buffer of the same size from shared pool
        var buf = System.Buffers.ArrayPool<byte>.Shared.Rent(pattern.Length);
        try
        {
            bool equal = true;
            for (int i = 0; i < pattern.Length; i++) if (buf[i] != pattern[i]) { equal = false; break; }
            Assert.That(equal, Is.False, "Rented buffer should not retain plaintext pattern");
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(buf, clearArray: true);
        }
    }

    private static (int count, List<int> lengths) ParseChunks(byte[] bytes, out int headerLen)
    {
        int offset = 0;
        // File header
        if (bytes.Length < 8) throw new InvalidDataException();
        using (Assert.EnterMultipleScope())
        {
            // Magic
            Assert.That(bytes[0], Is.EqualTo((byte)'C'));
            Assert.That(bytes[1], Is.EqualTo((byte)'T'));
            Assert.That(bytes[2], Is.EqualTo((byte)'N'));
            Assert.That(bytes[3], Is.EqualTo((byte)'1'));
        }
        int hdrLen = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(4, 4));
        headerLen = hdrLen;
        offset += hdrLen;
        int count = 0;
        var lens = new List<int>();
        while (offset < bytes.Length)
        {
            using (Assert.EnterMultipleScope())
            {
                // Chunk header
                Assert.That(bytes[offset + 0], Is.EqualTo((byte)'C'));
                Assert.That(bytes[offset + 1], Is.EqualTo((byte)'T'));
                Assert.That(bytes[offset + 2], Is.EqualTo((byte)'N'));
                Assert.That(bytes[offset + 3], Is.EqualTo((byte)'1'));
            }
            int chLen = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(offset + 4, 4));
            long plainLen = BinaryPrimitives.ReadInt64LittleEndian(bytes.AsSpan(offset + 8, 8));
            // skip to ciphertext
            offset += chLen;
            // skip ciphertext
            offset += (int)plainLen;
            count++;
            lens.Add((int)plainLen);
        }
        return (count, lens);
    }
}

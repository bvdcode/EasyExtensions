// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using EasyExtensions.Crypto.Models;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace EasyExtensions.Crypto.Tests;

[Category("Negativity")]
public class NegativityTests
{
    private const int TagSize = AesGcmStreamCipher.TagSize;
    private const int NonceSize = AesGcmStreamCipher.NonceSize;
    private const int MinChunk = AesGcmStreamCipher.MinChunkSize;

    private static byte[] ValidMasterKey() => [.. Enumerable.Range(0, 32).Select(i => (byte)i)];

    private static (AesGcmKeyHeader fileHeader, List<(AesGcmKeyHeader hdr, int cipherOffset)> chunks) ParseAllHeaders(byte[] encrypted)
    {
        using var ms = new MemoryStream(encrypted, writable: false);
        var fileHeader = AesGcmKeyHeader.FromStream(ms, NonceSize, TagSize);
        var chunks = new List<(AesGcmKeyHeader, int)>();
        while (ms.Position < ms.Length)
        {
            long posBefore = ms.Position;
            try
            {
                var ch = AesGcmKeyHeader.FromStream(ms, NonceSize, TagSize);
                int cipherOffset = (int)ms.Position;
                chunks.Add((ch, cipherOffset));
                ms.Position += ch.DataLength;
            }
            catch
            {
                ms.Position = posBefore;
                break;
            }
        }
        return (fileHeader, chunks);
    }

    [Test]
    public void Tamper_FileHeader_KeyId_ShouldFailEarly()
    {
        var cipher = new AesGcmStreamCipher(ValidMasterKey(), keyId: 12);
        byte[] data = [.. Enumerable.Range(0, MinChunk + 5_000).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        using var outEnc = new MemoryStream();
        cipher.EncryptAsync(input, outEnc, chunkSize: MinChunk).GetAwaiter().GetResult();

        var bytes = outEnc.ToArray();
        int keyIdOffset = 4 + 4 + 8; // magic + headerLen + dataLen
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(keyIdOffset, 4), 999);

        using var tampered = new MemoryStream(bytes, writable: false);
        using var outDec = new MemoryStream();
        Assert.ThrowsAsync<InvalidDataException>(async () => await cipher.DecryptAsync(tampered, outDec));
    }

    [Test]
    public void Tamper_FileHeader_EncryptedKey_ShouldFail()
    {
        var cipher = new AesGcmStreamCipher(ValidMasterKey(), keyId: 13);
        byte[] data = [.. Enumerable.Range(0, MinChunk + 1).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        using var outEnc = new MemoryStream();
        cipher.EncryptAsync(input, outEnc, chunkSize: MinChunk).GetAwaiter().GetResult();

        var bytes = outEnc.ToArray();
        int encKeyOffset = 4 + 4 + 8 + 4 + NonceSize + TagSize; // file header layout
        bytes[encKeyOffset] ^= 0xFF;

        using var tampered = new MemoryStream(bytes, writable: false);
        using var outDec = new MemoryStream();
        Assert.ThrowsAsync<AuthenticationTagMismatchException>(async () => await cipher.DecryptAsync(tampered, outDec));
    }

    [Test]
    public void Tamper_Chunk_Tag_ShouldFail_NoPayload()
    {
        var cipher = new AesGcmStreamCipher(ValidMasterKey(), keyId: 15);
        byte[] data = [.. Enumerable.Range(0, MinChunk + 10_000).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        using var outEnc = new MemoryStream();
        cipher.EncryptAsync(input, outEnc, chunkSize: MinChunk).GetAwaiter().GetResult();

        var bytes = outEnc.ToArray();
        var (_, chunks) = ParseAllHeaders(bytes);
        Assert.That(chunks, Has.Count.GreaterThan(0));

        int headerLen = 4 + 4 + 8 + 4 + TagSize; // compact chunk header (no nonce)
        int chunk0HeaderStart = chunks[0].cipherOffset - headerLen;
        int tagOffset = chunk0HeaderStart + 4 + 4 + 8 + 4; // tag starts right after keyId
        bytes[tagOffset] ^= 0xFF;

        using var tampered = new MemoryStream(bytes, writable: false);
        using var outDec = new MemoryStream();
        Assert.ThrowsAsync<AuthenticationTagMismatchException>(async () => await cipher.DecryptAsync(tampered, outDec));
        Assert.That(outDec.Length, Is.EqualTo(0));
    }

    [Test]
    public void Truncation_Fails_OnFileHeader_And_Chunk()
    {
        var cipher = new AesGcmStreamCipher(ValidMasterKey(), keyId: 2);
        byte[] data = [.. Enumerable.Range(0, MinChunk + 10_000).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        using var outEnc = new MemoryStream();
        cipher.EncryptAsync(input, outEnc, chunkSize: MinChunk).GetAwaiter().GetResult();

        var full = outEnc.ToArray();

        // Truncate inside ciphertext of first chunk
        var (_, chunks) = ParseAllHeaders(full);
        Assert.That(chunks, Has.Count.GreaterThan(0));
        int cut = chunks[0].cipherOffset + (int)(chunks[0].hdr.DataLength / 2);
        using var truncated1 = new MemoryStream(full.AsSpan(0, cut).ToArray(), writable: false);
        using var dec1 = new MemoryStream();
        Assert.ThrowsAsync<EndOfStreamException>(async () => await cipher.DecryptAsync(truncated1, dec1));

        // Truncate mid-file-header
        int fileHeaderLen = 4 + 4 + 8 + 4 + 4 + NonceSize + TagSize + 32;
        int cut2 = fileHeaderLen / 2;
        using var truncated2 = new MemoryStream(full.AsSpan(0, cut2).ToArray(), writable: false);
        using var dec2 = new MemoryStream();
        Assert.ThrowsAsync<EndOfStreamException>(async () => await cipher.DecryptAsync(truncated2, dec2));
    }
}

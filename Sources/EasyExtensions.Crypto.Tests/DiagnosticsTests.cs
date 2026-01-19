// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Buffers.Binary;
using System.Security.Cryptography;

namespace EasyExtensions.Crypto.Tests;

[Category("Diagnostics")]
public class DiagnosticsTests
{
    private static byte[] Key() => [.. Enumerable.Range(0, 32).Select(i => (byte)i)];

    // 16. Truncated inside chunk => CryptographicException
    [Test]
    public void Decrypt_TruncatedInsideChunk_MapsToCrypto()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 21, threads: 1);
        byte[] data = [.. Enumerable.Range(0, AesGcmStreamCipher.MinChunkSize * 3).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        using var enc = new MemoryStream();
        cipher.EncryptAsync(input, enc, chunkSize: AesGcmStreamCipher.MinChunkSize).GetAwaiter().GetResult();
        byte[] bytes = enc.ToArray();
        // Truncate in the middle of first ciphertext
        int headerLen = 4 + 4 + 8 + 4 + 4 + AesGcmStreamCipher.NonceSize + AesGcmStreamCipher.TagSize + AesGcmStreamCipher.KeySize;
        int chunkHdrLen = 4 + 4 + 8 + 4 + AesGcmStreamCipher.TagSize;
        int cut = headerLen + chunkHdrLen + (AesGcmStreamCipher.MinChunkSize / 2);
        using var truncated = new MemoryStream(bytes.AsSpan(0, cut).ToArray(), writable: false);
        using var outDec = new MemoryStream();
        Assert.ThrowsAsync<EndOfStreamException>(async () => await cipher.DecryptAsync(truncated, outDec));
    }

    // 17. Corrupt chunkHeader.Length => InvalidDataException
    [Test]
    public void Decrypt_CorruptChunkHeaderLength_FailsBeforeAEAD()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 22, threads: 1);
        byte[] data = [.. Enumerable.Range(0, AesGcmStreamCipher.MinChunkSize * 2).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        using var enc = new MemoryStream();
        cipher.EncryptAsync(input, enc, chunkSize: AesGcmStreamCipher.MinChunkSize).GetAwaiter().GetResult();
        byte[] bytes = enc.ToArray();
        int fileHeaderLen = 4 + 4 + 8 + 4 + 4 + AesGcmStreamCipher.NonceSize + AesGcmStreamCipher.TagSize + AesGcmStreamCipher.KeySize;
        // Decrease chunk header length by 8
        int len = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(fileHeaderLen + 4, 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(fileHeaderLen + 4, 4), len - 8);
        using var tampered = new MemoryStream(bytes, writable: false);
        using var outDec = new MemoryStream();
        Assert.ThrowsAsync<InvalidDataException>(async () => await cipher.DecryptAsync(tampered, outDec));
    }

    // 18. Duplicate and far out-of-order
    [Test]
    public void Encrypt_Consumer_OrderDiagnostics()
    {
        // Construct minimal fake sequence to hit consumer paths is non-trivial, so run real encrypt with small window
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 23, threads: 1);
        byte[] data = [.. Enumerable.Range(0, AesGcmStreamCipher.MinChunkSize + 100).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        using var enc = new MemoryStream();
        cipher.EncryptAsync(input, enc, chunkSize: AesGcmStreamCipher.MinChunkSize).GetAwaiter().GetResult();
        byte[] bytes = enc.ToArray();
        // Duplicate chunk: copy first chunk payload to appear twice
        int fh = 4 + 4 + 8 + 4 + 4 + AesGcmStreamCipher.NonceSize + AesGcmStreamCipher.TagSize + AesGcmStreamCipher.KeySize;
        int ch = 4 + 4 + 8 + 4 + AesGcmStreamCipher.TagSize;
        // Build stream with chunk0 twice
        using var dup = new MemoryStream();
        dup.Write(bytes, 0, fh + ch + AesGcmStreamCipher.MinChunkSize);
        dup.Write(bytes, fh, ch + AesGcmStreamCipher.MinChunkSize);
        dup.Position = 0;
        using var outDec = new MemoryStream();
        Assert.ThrowsAsync<AuthenticationTagMismatchException>(async () => await cipher.DecryptAsync(dup, outDec));
    }
}

// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using EasyExtensions.Crypto.Tests.TestUtils;
using System.Security.Cryptography;

namespace EasyExtensions.Crypto.Tests;

[Category("Streaming-Pipe")]
public class AesGcmStreamCipherPipeTests
{
    private static byte[] Key() => [.. Enumerable.Range(0, 32).Select(i => (byte)i)];

    [Test]
    public async Task EncryptAsync_StreamOverload_DecryptsCorrectly()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 11, threads: 2);
        byte[] data = [.. Enumerable.Range(0, 3 * AesGcmStreamCipher.MinChunkSize + 123).Select(i => (byte)(i & 0xFF))];

        using var input1 = new MemoryStream(data);
        using var directOut = new MemoryStream();
        await cipher.EncryptAsync(input1, directOut, chunkSize: AesGcmStreamCipher.MinChunkSize);
        directOut.Position = 0;
        using var directPlain = new MemoryStream();
        await cipher.DecryptAsync(directOut, directPlain);

        using var input2 = new MemoryStream(data);
        var pipeStream = await cipher.EncryptAsync(input2, chunkSize: AesGcmStreamCipher.MinChunkSize);
        using var collected = new MemoryStream();
        await pipeStream.CopyToAsync(collected);
        collected.Position = 0;
        using var pipePlain = new MemoryStream();
        await cipher.DecryptAsync(collected, pipePlain);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(directPlain.ToArray(), Is.EqualTo(data));
            Assert.That(pipePlain.ToArray(), Is.EqualTo(data));
        }
    }

    [Test]
    public async Task RoundTrip_EncryptDecrypt_StreamOverloads()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 12, threads: 2);
        byte[] data = [.. Enumerable.Range(0, 5 * AesGcmStreamCipher.MinChunkSize + 777).Select(i => (byte)(i & 0xFF))];

        using var input = new MemoryStream(data);
        var encStream = await cipher.EncryptAsync(input, chunkSize: AesGcmStreamCipher.MinChunkSize);
        using var ciphertextCollected = new MemoryStream();
        await encStream.CopyToAsync(ciphertextCollected);

        ciphertextCollected.Position = 0;
        var decStream = await cipher.DecryptAsync(ciphertextCollected);
        using var plaintextCollected = new MemoryStream();
        await decStream.CopyToAsync(plaintextCollected);

        Assert.That(plaintextCollected.ToArray(), Is.EqualTo(data));
    }

    [Test]
    public async Task EncryptAsync_StreamOverload_Cancellation()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 13, threads: 2);
        // More chunks, minimal chunk size to increase chance of pending read
        byte[] data = [.. Enumerable.Range(0, 16 * AesGcmStreamCipher.MinChunkSize).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(25);

        var stream = await cipher.EncryptAsync(input, chunkSize: AesGcmStreamCipher.MinChunkSize, ct: cts.Token);
        byte[] buffer = new byte[8 * 1024];
        TaskCanceledException? caught = null;
        long totalRead = 0;
        try
        {
            while (true)
            {
                int r = await stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                if (r == 0) break;
                totalRead += r;
                await Task.Delay(5); // slow down to increase chance of cancellation
            }
        }
        catch (TaskCanceledException ex)
        {
            caught = ex;
        }

        Assert.That(caught != null || (cts.IsCancellationRequested && totalRead < data.Length), "Expected cancellation to abort or truncate encryption stream.");
    }

    [Test]
    public async Task DecryptAsync_StreamOverload_Cancellation()
    {
        var key = Key();
        var encCipher = new AesGcmStreamCipher(key, keyId: 14, threads: 2);
        var decCipher = new AesGcmStreamCipher(key, keyId: 14, threads: 2);
        byte[] data = [.. Enumerable.Range(0, 16 * AesGcmStreamCipher.MinChunkSize).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        using var encrypted = new MemoryStream();
        await encCipher.EncryptAsync(input, encrypted, chunkSize: AesGcmStreamCipher.MinChunkSize);
        encrypted.Position = 0;

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(25);
        var decStream = await decCipher.DecryptAsync(encrypted, ct: cts.Token);
        byte[] buffer = new byte[8 * 1024];
        TaskCanceledException? caught = null;
        long totalRead = 0;
        try
        {
            while (true)
            {
                int r = await decStream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                if (r == 0) break;
                totalRead += r;
                await Task.Delay(5); // slow down to increase chance of cancellation
            }
        }
        catch (TaskCanceledException ex)
        {
            caught = ex;
        }

        Assert.That(caught != null || (cts.IsCancellationRequested && totalRead < data.Length), "Expected cancellation to abort or truncate decryption stream.");
    }

    [Test]
    public async Task EncryptAsync_StreamOverload_NonSeekableInput()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 15);
        byte[] data = [.. Enumerable.Range(0, 500_000).Select(i => (byte)(i & 0xFF))];
        using var inner = new MemoryStream(data);
        using var nonSeek = new NonSeekableReadStream(inner);

        var stream = await cipher.EncryptAsync(nonSeek, chunkSize: AesGcmStreamCipher.MinChunkSize);
        using var collected = new MemoryStream();
        await stream.CopyToAsync(collected);

        collected.Position = 0;
        var decStream = await cipher.DecryptAsync(collected);
        using var plainCollected = new MemoryStream();
        await decStream.CopyToAsync(plainCollected);

        Assert.That(plainCollected.ToArray(), Is.EqualTo(data));
    }

    [Test]
    public async Task DecryptAsync_StreamOverload_Tamper_Throws()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 16);
        byte[] data = [.. Enumerable.Range(0, 3 * AesGcmStreamCipher.MinChunkSize + 10).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        var encStream = await cipher.EncryptAsync(input, chunkSize: AesGcmStreamCipher.MinChunkSize);
        using var ciphertextCollected = new MemoryStream();
        await encStream.CopyToAsync(ciphertextCollected);

        var bytes = ciphertextCollected.ToArray();
        int headerLen = EasyExtensions.Crypto.Internals.AesGcmStreamFormat.ComputeFileHeaderLength(AesGcmStreamCipher.NonceSize, AesGcmStreamCipher.TagSize, AesGcmStreamCipher.KeySize);
        if (bytes.Length > headerLen + 5) bytes[headerLen + 5] ^= 0xFF; // corrupt
        using var tampered = new MemoryStream(bytes);
        var decStream = await cipher.DecryptAsync(tampered);
        using var sink = new MemoryStream();
        Assert.That(async () => await decStream.CopyToAsync(sink),
            Throws.TypeOf<InvalidDataException>().Or.TypeOf<CryptographicException>());
    }
}

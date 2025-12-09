// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using EasyExtensions.Crypto.Tests.TestUtils;

namespace EasyExtensions.Crypto.Tests;

[Category("Streaming")]
public class StreamingTests
{
    private static byte[] Key() => [.. Enumerable.Range(0, 32).Select(i => (byte)i)];

    [Test]
    public async Task EncryptDecrypt_WithNonSeekable_Streams()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 7, threads: 2);
        byte[] data = [.. Enumerable.Range(0, 500_000).Select(i => (byte)(i & 0xFF))];

        using var inner = new MemoryStream(data);
        using var nonSeek = new NonSeekableReadStream(inner);
        using var encrypted = new MemoryStream();
        await cipher.EncryptAsync(nonSeek, encrypted, chunkSize: AesGcmStreamCipher.MinChunkSize);

        encrypted.Position = 0;
        using var decrypted = new MemoryStream();
        await cipher.DecryptAsync(encrypted, decrypted);

        Assert.That(decrypted.ToArray(), Is.EqualTo(data));
    }

    [Test]
    public void Encrypt_Cancellation_MidPipeline_NoLeaks()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 5, threads: 2);
        byte[] data = [.. Enumerable.Range(0, AesGcmStreamCipher.MinChunkSize * 3).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        using var slowOut = new SlowWriteStream(new MemoryStream(), delayMs: 10);
        using var cts = new CancellationTokenSource();

        long before = GC.GetAllocatedBytesForCurrentThread();
        var task = cipher.EncryptAsync(input, slowOut, chunkSize: AesGcmStreamCipher.MinChunkSize, ct: cts.Token);
        // cancel after small delay to let pipeline spin up
        Task.Delay(30).ContinueWith(_ => cts.Cancel());

        Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        long after = GC.GetAllocatedBytesForCurrentThread();
        // Not a strict zero-alloc, but ensure we didn't balloon allocations by > 10MB due to leaks
        Assert.That(after - before, Is.LessThan(10L * 1024 * 1024));
    }

    [Test]
    public void Decrypt_Cancellation_MidPipeline_NoLeaks()
    {
        var key = Key();
        var enc = new AesGcmStreamCipher(key, keyId: 6, threads: 2);
        var dec = new AesGcmStreamCipher(key, keyId: 6, threads: 2);
        // Use more data to ensure decrypt runs long enough to observe cancellation reliably
        byte[] data = [.. Enumerable.Range(0, AesGcmStreamCipher.MinChunkSize * 32).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        using var ciphertext = new MemoryStream();
        enc.EncryptAsync(input, ciphertext, chunkSize: AesGcmStreamCipher.MinChunkSize).GetAwaiter().GetResult();
        ciphertext.Position = 0;

        using var slowOut = new SlowWriteStream(new MemoryStream(), delayMs: 25);
        using var cts = new CancellationTokenSource();

        long before = GC.GetAllocatedBytesForCurrentThread();
        // Kick off decryption; request cancellation soon after to interrupt mid-pipeline
        var task = dec.DecryptAsync(ciphertext, slowOut, ct: cts.Token);
        cts.CancelAfter(50);
        Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        long after = GC.GetAllocatedBytesForCurrentThread();
        Assert.That(after - before, Is.LessThan(10L * 1024 * 1024));
    }

    [Test]
    public void HugeFile_SyntheticStream_HeaderAndIndices_LongVsInt()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 8);
        long huge = 6L * 1024 * 1024 * 1024; // 6 GB
        using var fake = new SeekableSyntheticReadStream(huge);
        using var outEnc = new MemoryStream();
        // Only header will be written (input immediately EOF), but code path uses long for lengths
        cipher.EncryptAsync(fake, outEnc, chunkSize: AesGcmStreamCipher.DefaultChunkSize).GetAwaiter().GetResult();
        outEnc.Position = 0;
        // Should be able to parse headers without overflow
        var hdr = EasyExtensions.Crypto.Models.AesGcmKeyHeader.FromStream(outEnc, AesGcmStreamCipher.NonceSize, AesGcmStreamCipher.TagSize);
        Assert.That(hdr.DataLength, Is.EqualTo(huge));
    }

    [Test]
    public void HotPaths_DoNotAllocate_Significantly()
    {
        var key = Key();
        var cipher = new AesGcmStreamCipher(key, keyId: 9, threads: 2);
        byte[] data = [.. Enumerable.Range(0, AesGcmStreamCipher.MinChunkSize).Select(i => (byte)(i & 0xFF))];
        using var input = new MemoryStream(data);
        using var output = new DevNullStream();

        // warm-up
        cipher.EncryptAsync(input, output, chunkSize: AesGcmStreamCipher.MinChunkSize).GetAwaiter().GetResult();
        input.Position = 0;

        long before = GC.GetAllocatedBytesForCurrentThread();
        cipher.EncryptAsync(input, output, chunkSize: AesGcmStreamCipher.MinChunkSize).GetAwaiter().GetResult();
        long after = GC.GetAllocatedBytesForCurrentThread();

        // Allow small control-plane allocations (<32KB)
        Assert.That(after - before, Is.LessThan(32 * 1024));
    }
}

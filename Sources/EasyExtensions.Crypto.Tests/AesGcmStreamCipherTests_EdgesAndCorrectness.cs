// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using System.Text;

namespace EasyExtensions.Crypto.Tests;

public class AesGcmStreamCipherTests_EdgesAndCorrectness
{
    private static readonly byte[] MasterKey = [.. Enumerable.Range(0, 32).Select(i => (byte)i)];

    [TestCase(0)]
    [TestCase(1)]
    [Category("Edge")]
    public async Task EncryptDecrypt_EdgeCases_ShouldWork(int bytes)
    {
        byte[] source = new byte[Math.Max(1, bytes)];
        for (int i = 0; i < bytes; i++) source[i] = (byte)i;

        using MemoryStream inputStream = bytes == 0 ? new MemoryStream([]) : new MemoryStream(source, 0, bytes, writable: false, publiclyVisible: true);
        using MemoryStream encryptedStream = new();
        using MemoryStream decryptedStream = new();

        var cipher = new AesGcmStreamCipher(MasterKey);

        await cipher.EncryptAsync(inputStream, encryptedStream);
        encryptedStream.Position = 0;
        await cipher.DecryptAsync(encryptedStream, decryptedStream);

        Assert.That(decryptedStream.Length, Is.EqualTo(bytes));
        if (bytes > 0)
        {
            Assert.That(decryptedStream.ToArray(), Is.EqualTo(source.AsSpan(0, bytes).ToArray()));
        }
    }

    [Test]
    public async Task EncryptDecrypt_Correctness_RoundTrip_ShouldMatch()
    {
        var text = string.Join(',', Enumerable.Range(0, 2000));
        var data = Encoding.UTF8.GetBytes(text);

        using var input = new MemoryStream(data);
        using var encrypted = new MemoryStream();
        using var decrypted = new MemoryStream();

        var cipher = new AesGcmStreamCipher(MasterKey);
        await cipher.EncryptAsync(input, encrypted);
        encrypted.Position = 0;
        await cipher.DecryptAsync(encrypted, decrypted);

        Assert.That(decrypted.ToArray(), Is.EqualTo(data));
    }
}

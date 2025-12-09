// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using EasyExtensions.Crypto.Models;
using EasyExtensions.Crypto.Tests.TestUtils;
using System.Security.Cryptography;
using System.Text;

namespace EasyExtensions.Crypto.Tests;

[Category("Core")]
public class AesGcmStreamCipherTests
{
    private const int TagSize = AesGcmStreamCipher.TagSize;
    private const int NonceSize = AesGcmStreamCipher.NonceSize;
    private const int MinChunkSize = AesGcmStreamCipher.MinChunkSize;
    private const int MaxChunkSize = AesGcmStreamCipher.MaxChunkSize;

    private static byte[] CreateRandomBytes(int size, int seed = 12345)
    {
        var rng = new Random(seed);
        var data = new byte[size];
        rng.NextBytes(data);
        return data;
    }

    private static AesGcmStreamCipher CreateCipher(byte[] masterKey, int keyId = 1)
        => new(masterKey, keyId);

    private static async Task<MemoryStream> EncryptToMemoryAsync(AesGcmStreamCipher cipher, Stream input, int chunkSize)
    {
        var output = new MemoryStream();
        await cipher.EncryptAsync(input, output, chunkSize);
        output.Position = 0;
        return output;
    }

    private static async Task<MemoryStream> DecryptToMemoryAsync(AesGcmStreamCipher cipher, Stream input)
    {
        var output = new MemoryStream();
        await cipher.DecryptAsync(input, output);
        output.Position = 0;
        return output;
    }

    private static (AesGcmKeyHeader fileHeader, AesGcmKeyHeader? firstChunkHeader, int firstCiphertextOffset) ParseHeaders(byte[] encrypted)
    {
        using var ms = new MemoryStream(encrypted, writable: false);
        var fileHeader = AesGcmKeyHeader.FromStream(ms, NonceSize, TagSize);

        AesGcmKeyHeader? chunkHeader = null;
        int firstCiphertextOffset = (int)ms.Position;
        try
        {
            chunkHeader = AesGcmKeyHeader.FromStream(ms, NonceSize, TagSize);
            firstCiphertextOffset = (int)ms.Position;
        }
        catch (EndOfStreamException)
        {
            // no chunks
        }

        return (fileHeader, chunkHeader, firstCiphertextOffset);
    }

    private static byte[] ValidMasterKey()
    {
        // 32 bytes deterministic key
        return [.. Enumerable.Range(0, 32).Select(i => (byte)i)];
    }

    [Test]
    public void Constructor_InvalidMasterKey_Throws()
    {
        var tooShort = new byte[16];
        Assert.Throws<ArgumentException>(() => CreateCipher(tooShort));
    }

    [Test]
    public void Constructor_InvalidKeyId_Throws()
    {
        var mk = ValidMasterKey();
        Assert.Throws<ArgumentOutOfRangeException>(() => new AesGcmStreamCipher(mk, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AesGcmStreamCipher(mk, -1));
    }

    [Test]
    public void Encrypt_InvalidChunkSize_Throws_BelowMin()
    {
        var mk = ValidMasterKey();
        var cipher = CreateCipher(mk);
        using var input = new MemoryStream(CreateRandomBytes(1024));
        using var output = new MemoryStream();
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await cipher.EncryptAsync(input, output, MinChunkSize - 1));
    }

    [Test]
    public void Encrypt_InvalidChunkSize_Throws_AboveMax()
    {
        var mk = ValidMasterKey();
        var cipher = CreateCipher(mk);
        using var input = new MemoryStream(CreateRandomBytes(1024));
        using var output = new MemoryStream();
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await cipher.EncryptAsync(input, output, MaxChunkSize + 1));
    }

    [Test]
    public async Task RoundTrip_SmallData_DefaultChunk()
    {
        var mk = ValidMasterKey();
        var cipher = CreateCipher(mk, keyId: 7);

        var plaintext = Encoding.UTF8.GetBytes("Hello AES-GCM streaming!");
        using var input = new MemoryStream(plaintext);
        var encrypted = await EncryptToMemoryAsync(cipher, input, chunkSize: 1_048_576);

        encrypted.Position = 0;
        var decrypted = await DecryptToMemoryAsync(cipher, encrypted);

        Assert.That(plaintext, Is.EqualTo(decrypted.ToArray()));
    }

    [TestCase(65_536)]
    [TestCase(131_072)]
    [TestCase(1_048_576)]
    public async Task RoundTrip_LargeData_CustomChunkSizes(int chunkSize)
    {
        var mk = ValidMasterKey();
        var cipher = CreateCipher(mk, keyId: 3);

        int dataLen = (int)((chunkSize * 2.5) + 123); // guarantee multiple chunks
        var data = CreateRandomBytes(dataLen);
        using var input = new MemoryStream(data);

        var encrypted = await EncryptToMemoryAsync(cipher, input, chunkSize);

        encrypted.Position = 0;
        var decrypted = await DecryptToMemoryAsync(cipher, encrypted);

        Assert.That(data, Is.EqualTo(decrypted.ToArray()));
    }

    [Test]
    public async Task Encrypt_WritesHeader_WithKeyId_AndDataLength()
    {
        var mk = ValidMasterKey();
        int keyId = 42;
        var cipher = CreateCipher(mk, keyId);

        var data = CreateRandomBytes(200_000);
        using var input = new MemoryStream(data);
        var encrypted = await EncryptToMemoryAsync(cipher, input, chunkSize: 65_536);

        var bytes = encrypted.ToArray();
        var (fileHeader, firstChunkHeader, _) = ParseHeaders(bytes);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fileHeader.KeyId, Is.EqualTo(keyId));
            Assert.That(fileHeader.DataLength, Is.EqualTo(data.Length));
            Assert.That(fileHeader.Nonce, Has.Length.EqualTo(NonceSize));
            Assert.That(fileHeader.Tag, Has.Length.EqualTo(TagSize));
            Assert.That(firstChunkHeader, Is.Not.Null);
        }
        using (Assert.EnterMultipleScope())
        {
            // Compact chunk header: nonce is not serialized
            Assert.That(firstChunkHeader!.Value.Nonce, Has.Length.EqualTo(0));
            Assert.That(firstChunkHeader!.Value.Tag, Has.Length.EqualTo(TagSize));
            Assert.That(firstChunkHeader!.Value.DataLength, Is.GreaterThan(0));
        }
    }

    [Test]
    public async Task Encrypt_WritesHeader_NonSeekable_DataLengthZero()
    {
        var mk = ValidMasterKey();
        var cipher = CreateCipher(mk, keyId: 9);

        var data = CreateRandomBytes(100_000);
        using var inner = new MemoryStream(data);
        using var nonSeek = new NonSeekableReadStream(inner);

        var encrypted = new MemoryStream();
        await cipher.EncryptAsync(nonSeek, encrypted, chunkSize: 65_536);
        var bytes = encrypted.ToArray();

        var (fileHeader, _, _) = ParseHeaders(bytes);
        Assert.That(fileHeader.DataLength, Is.Zero);
    }

    [Test]
    public void Decrypt_Fails_OnTamperedCiphertext()
    {
        var mk = ValidMasterKey();
        var cipher = CreateCipher(mk);

        var data = CreateRandomBytes(120_000);
        using var input = new MemoryStream(data);
        var encrypted = EncryptToMemoryAsync(cipher, input, chunkSize: 65_536).GetAwaiter().GetResult();

        var bytes = encrypted.ToArray();
        var (_, firstChunkHeader, firstCipherOffset) = ParseHeaders(bytes);
        Assert.That(firstChunkHeader, Is.Not.Null, "Expected at least one chunk.");

        // Flip one byte in the first chunk ciphertext
        if (firstChunkHeader!.Value.DataLength > 0)
        {
            bytes[firstCipherOffset] ^= 0xFF;
        }

        using var tampered = new MemoryStream(bytes, writable: false);
        using var output = new MemoryStream();
        Assert.ThrowsAsync<AuthenticationTagMismatchException>(async () => await cipher.DecryptAsync(tampered, output));
    }

    [Test]
    public void Encrypt_Cancellation_Throws()
    {
        var mk = ValidMasterKey();
        var cipher = CreateCipher(mk);

        var data = CreateRandomBytes(1_000_000);
        using var input = new MemoryStream(data);
        using var output = new MemoryStream();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<TaskCanceledException>(async () => await cipher.EncryptAsync(input, output, chunkSize: 65_536, ct: cts.Token));
    }
}

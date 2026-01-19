// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Crypto.Internals;
using System.Buffers.Binary;

namespace EasyExtensions.Crypto.Tests;

[Category("Format")]

public class FormatInvariantTests
{
    [Test]
    public void ComposeNonce_RoundTrip_NoCollisions_ForFirstMillion()
    {
        uint prefix = 0xA1B2C3D4;
        var seen = new HashSet<ulong>(1_000_000);
        Span<byte> nonce = stackalloc byte[AesGcmStreamCipher.NonceSize];

        // We cannot capture stackalloc span in lambdas; run assertions inline
        for (long i = 0; i < 1_000_000; i++)
        {
            AesGcmStreamFormat.ComposeNonce(nonce, prefix, i);
            var pfx = BinaryPrimitives.ReadUInt32LittleEndian(nonce[..4]);
            ulong ctr = BinaryPrimitives.ReadUInt64LittleEndian(nonce[4..]);
            Assert.Multiple(() =>
            {
                Assert.That(pfx, Is.EqualTo(prefix));
                Assert.That(seen.Add(ctr), Is.True, "Duplicate counter detected at i=" + i);
                Assert.That(ctr, Is.EqualTo((ulong)i));
            });
        }
    }

    [Test]
    public void ComposeNonce_Overflow_Throws()
    {
        uint prefix = 0x01020304;
        Span<byte> nonce = stackalloc byte[AesGcmStreamCipher.NonceSize];
        bool thrown = false;
        try
        {
            AesGcmStreamFormat.ComposeNonce(nonce, prefix, -1L);
        }
        catch (InvalidOperationException)
        {
            thrown = true;
        }
        Assert.That(thrown, Is.True);
    }

    [Test]
    public void AadPrefix_IsConstant_MutableFieldsChangeOnly()
    {
        int keyId = 123;
        Span<byte> aad = stackalloc byte[32];
        AesGcmStreamFormat.InitAadPrefix(aad, keyId);
        byte[] prefix = aad[..12].ToArray();

        long[] indices = [0L, 1L, 123456789L];
        long[] lengths = [0L, 1L, 42L, 8_388_608L];
        foreach (var idx in indices)
        {
            foreach (var len in lengths)
            {
                AesGcmStreamFormat.FillAadMutable(aad, idx, len);
                // Compute values without lambdas capturing span
                var prefixNow = aad[..12].ToArray();
                var idxNow = BinaryPrimitives.ReadInt64LittleEndian(aad.Slice(12, 8));
                var lenNow = BinaryPrimitives.ReadInt64LittleEndian(aad.Slice(20, 8));
                var zeroNow = BinaryPrimitives.ReadInt32LittleEndian(aad.Slice(28, 4));

                Assert.Multiple(() =>
                {
                    Assert.That(prefixNow, Is.EqualTo(prefix));
                    Assert.That(idxNow, Is.EqualTo(idx));
                    Assert.That(lenNow, Is.EqualTo(len));
                    Assert.That(zeroNow, Is.EqualTo(0));
                });
            }
        }
    }
}

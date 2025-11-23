// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using EasyExtensions.Crypto.Tests.TestUtils;
using System.Diagnostics;

namespace EasyExtensions.Crypto.Tests
{
    [Category("Performance")]
    [NonParallelizable]
    public class PerformanceTests
    {
        private static readonly Lock _initLock = new();
        private static byte[]? _sharedData;
        private static byte[]? _masterKey;
        private const int OneMb = 1024 * 1024;
        private const int TestDataSizeMb = 1000;
        private const int Iterations = 2;
        private static readonly int[] chunkSizesInKBytes = [64, 128, 512, 1024, 4096, 8192, 16384];

        [SetUp]
        public void SetUp()
        {
            if (_sharedData != null && _masterKey != null) return;

            lock (_initLock)
            {
                if (_sharedData != null && _masterKey != null) return;

                // Fixed master key once for all tests
                _masterKey = new byte[32];
                for (int i = 0; i < _masterKey.Length; i++) _masterKey[i] = (byte)i;

                // Prepare shared plaintext buffer (1 GB)
                int sizeBytes = TestDataSizeMb * OneMb;
                byte[] data = new byte[sizeBytes];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)(i & 0xFF);
                }
                _sharedData = data;
            }
        }

        [Test]
        public async Task Encrypt_ThreadSweep_ChunkSweep()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(_sharedData, Is.Not.Null);
                Assert.That(_masterKey, Is.Not.Null);
            }

            byte[] source = _sharedData!;
            byte[] masterKey = _masterKey!;
            int totalBytes = TestDataSizeMb * OneMb;

            int[] threadCounts = [.. GetThreadSweep()];
            int[] chunkSizes = GetChunkSweep();

            TestContext.Out.WriteLine("=== ENCRYPTION THREAD/CHUNK SWEEP ===");
            TestContext.Out.WriteLine($"Data size: {TestDataSizeMb} MB");
            TestContext.Out.WriteLine($"Threads: {string.Join(", ", threadCounts)}");
            TestContext.Out.WriteLine($"Chunk sizes: {string.Join(", ", chunkSizes.Select(x => $"{x / (double)OneMb:F1}MB"))}");
            TestContext.Out.WriteLine("Threads | ChunkMB | Avg MB/s");

            foreach (int threads in threadCounts)
            {
                foreach (int chunkSize in chunkSizes)
                {
                    List<double> throughputs = [];
                    for (int i = 0; i < Iterations; i++)
                    {
                        var cipher = new AesGcmStreamCipher(masterKey, keyId: 1, threads: threads);
                        using var inputStream = new MemoryStream(source, 0, totalBytes, writable: false, publiclyVisible: true);
                        using var encryptedStream = new DevNullStream();
                        long t0 = Stopwatch.GetTimestamp();
                        await cipher.EncryptAsync(inputStream, encryptedStream, chunkSize: chunkSize);
                        long t1 = Stopwatch.GetTimestamp();
                        double timeSeconds = (t1 - t0) / (double)Stopwatch.Frequency;
                        double throughputMBps = TestDataSizeMb / timeSeconds;
                        throughputs.Add(throughputMBps);
                    }
                    double avg = throughputs.Average();
                    TestContext.Out.WriteLine($"{threads,7} | {chunkSize / (double)OneMb,7:F3} | {avg,9:F1}");
                }
            }
        }

        [Test]
        public async Task Decrypt_ThreadSweep_ChunkSweep()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(_sharedData, Is.Not.Null);
                Assert.That(_masterKey, Is.Not.Null);
            }

            byte[] source = _sharedData!;
            byte[] masterKey = _masterKey!;
            int totalBytes = TestDataSizeMb * OneMb;

            // Prepare one ciphertext for all decrypt sweeps
            byte[] encryptedPayload;
            {
                var cipher = new AesGcmStreamCipher(masterKey, keyId: 1);
                using var input = new MemoryStream(source, 0, totalBytes, writable: false, publiclyVisible: true);
                using var encrypted = new MemoryStream(capacity: totalBytes + 4096);
                await cipher.EncryptAsync(input, encrypted);
                encryptedPayload = encrypted.ToArray();
            }

            int[] threadCounts = [.. GetThreadSweep()];
            int[] chunkSizes = GetChunkSweep();

            TestContext.Out.WriteLine("=== DECRYPTION THREAD/CHUNK SWEEP ===");
            TestContext.Out.WriteLine($"Data size: {TestDataSizeMb} MB");
            TestContext.Out.WriteLine($"Threads: {string.Join(", ", threadCounts)}");
            TestContext.Out.WriteLine($"Chunk sizes: {string.Join(", ", chunkSizes.Select(x => $"{x / (double)OneMb:F1}MB"))}");
            TestContext.Out.WriteLine("Threads | ChunkMB | Avg MB/s");

            foreach (int threads in threadCounts)
            {
                foreach (int chunkSize in chunkSizes)
                {
                    List<double> throughputs = [];
                    for (int i = 0; i < Iterations; i++)
                    {
                        var cipher = new AesGcmStreamCipher(masterKey, keyId: 1, threads: threads);
                        using var encryptedStream = new MemoryStream(encryptedPayload, writable: false);
                        var decryptedStream = new DevNullStream();
                        long t0 = Stopwatch.GetTimestamp();
                        await cipher.DecryptAsync(encryptedStream, decryptedStream);
                        long t1 = Stopwatch.GetTimestamp();
                        double timeSeconds = (t1 - t0) / (double)Stopwatch.Frequency;
                        double throughputMBps = TestDataSizeMb / timeSeconds;
                        throughputs.Add(throughputMBps);
                    }
                    double avg = throughputs.Average();
                    TestContext.Out.WriteLine($"{threads,7} | {chunkSize / (double)OneMb,7:F3} | {avg,9:F1}");
                }
            }
        }

        private static IEnumerable<int> GetThreadSweep()
        {
            int threads = Math.Max(8, Environment.ProcessorCount);
            for (int i = 1; i < threads; i++)
            {
                if ((i & (i - 1)) == 0) // power of two
                {
                    yield return i;
                }
            }
        }

        private static int[] GetChunkSweep()
        {
            return [.. chunkSizesInKBytes.Select(x => x * 1024)];
        }
    }
}

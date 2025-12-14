// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EasyExtensions.Crypto.Internals.Pipelines
{
    internal class EncryptionPipeline(Stream input, Stream output, byte[] fileKey,
        uint noncePrefix, int chunkSize, int threads, int keyId, int nonceSize, int tagSize, int windowCap, ArrayPool<byte> pool)
    {
        public async Task RunAsync(CancellationToken ct)
        {
            var jobCh = Channel.CreateBounded<EncryptionJob>(new BoundedChannelOptions(threads * 4) { SingleWriter = true, SingleReader = false, FullMode = BoundedChannelFullMode.Wait });
            var resCh = Channel.CreateBounded<EncryptionResult>(new BoundedChannelOptions(threads * 4) { SingleWriter = false, SingleReader = true, FullMode = BoundedChannelFullMode.Wait });

            int jobCap = threads * 4;
            int resCap = threads * 4;
            int window = Math.Min(Math.Max(4, threads * 4), windowCap);
            // Conservative upper-bound for concurrently active buffers (input + output + worker in-flight + reordering backlog).
            // We explicitly include a multiple of windowCap to tolerate out-of-order accumulation under contention.
            int allowedBacklog = Math.Min(windowCap * 2, 32768);
            int maxCount = checked(jobCap + threads + resCap + allowedBacklog);

            static int NextPow2(int x)
            {
                x--;
                x |= x >> 1;
                x |= x >> 2;
                x |= x >> 4;
                x |= x >> 8;
                x |= x >> 16;
                x++;
                return x < 16 ? 16 : x;
            }
            long estSize = NextPow2(chunkSize);
            long maxBytes = estSize * maxCount;
            using var scope = new BufferScope(pool, maxCount: maxCount, maxBytes: maxBytes);

            var producer = ProduceAsync(jobCh.Writer, scope, ct);
            var workers = StartWorkersAsync(jobCh.Reader, resCh.Writer, scope, ct);
            var consumer = ConsumeAsync(resCh.Reader, scope, ct);

            try
            {
                await producer.ConfigureAwait(false);
                await Task.WhenAll(workers).ConfigureAwait(false);
            }
            finally
            {
                resCh.Writer.TryComplete();
                await consumer.ConfigureAwait(false);
            }
        }

        private Task ProduceAsync(ChannelWriter<EncryptionJob> writer, BufferScope scope, CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                long idx = 0;
                try
                {
                    while (true)
                    {
                        ct.ThrowIfCancellationRequested();
                        byte[] buffer = scope.Rent(chunkSize);
                        int read = 0;
                        try
                        {
                            read = await input.ReadAsync(buffer.AsMemory(0, chunkSize), ct).ConfigureAwait(false);
                        }
                        catch
                        {
                            scope.Recycle(buffer);
                            throw;
                        }
                        if (read <= 0)
                        {
                            scope.Recycle(buffer);
                            break;
                        }
                        if (unchecked((ulong)idx) == ulong.MaxValue)
                        {
                            scope.Recycle(buffer);
                            throw new InvalidOperationException("Maximum number of chunks per file is 2^64-1. Counter reached ulong.MaxValue.");
                        }
                        await writer.WriteAsync(new EncryptionJob(idx++, buffer, read), ct).ConfigureAwait(false);
                    }
                }
                finally
                {
                    writer.TryComplete();
                }
            }, ct);
        }

        private Task[] StartWorkersAsync(ChannelReader<EncryptionJob> reader, ChannelWriter<EncryptionResult> writer, BufferScope scope, CancellationToken ct)
        {
            var tasks = new Task[threads];
            for (int i = 0; i < threads; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    using var gcm = new AesGcm(fileKey, tagSize);
                    byte[] nonceBuffer = new byte[nonceSize];
                    byte[] aad = new byte[32];
                    AesGcmStreamFormat.InitAadPrefix(aad, keyId);
                    await foreach (var job in reader.ReadAllAsync(ct))
                    {
                        ct.ThrowIfCancellationRequested();
                        byte[] cipher = scope.Rent(job.DataLength);
                        try
                        {
                            AesGcmStreamFormat.ComposeNonce(nonceBuffer, noncePrefix, job.Index);
                            AesGcmStreamFormat.FillAadMutable(aad, job.Index, job.DataLength);
                            Tag128 tag = default;
                            Span<byte> tagSpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref tag, 1));
                            gcm.Encrypt(nonceBuffer, job.DataBuffer.AsSpan(0, job.DataLength), cipher.AsSpan(0, job.DataLength), tagSpan, aad);
                            await writer.WriteAsync(new EncryptionResult(job.Index, tag, cipher, job.DataLength), ct).ConfigureAwait(false);
                        }
                        finally
                        {
                            scope.Recycle(job.DataBuffer);
                        }
                    }
                }, ct);
            }
            return tasks;
        }

        private Task ConsumeAsync(ChannelReader<EncryptionResult> reader, BufferScope scope, CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                var pending = new SortedDictionary<long, EncryptionResult>();
                long next = 0;
                while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
                {
                    while (reader.TryRead(out var res))
                    {
                        pending[res.Index] = res;
                        while (pending.TryGetValue(next, out var ready))
                        {
                            int headerLen = AesGcmStreamFormat.ComputeChunkHeaderLength(tagSize);
                            byte[] headerBuf = pool.Rent(headerLen);
                            try
                            {
                                AesGcmStreamFormat.BuildChunkHeader(headerBuf.AsSpan(0, headerLen), keyId, ready.Tag, ready.DataLength, tagSize);
                                await output.WriteAsync(headerBuf.AsMemory(0, headerLen), ct).ConfigureAwait(false);
                            }
                            finally
                            {
                                pool.Return(headerBuf, clearArray: false);
                            }
                            await output.WriteAsync(ready.Data.AsMemory(0, ready.DataLength), ct).ConfigureAwait(false);
                            scope.Recycle(ready.Data);
                            pending.Remove(next);
                            next++;
                        }
                    }
                }
                while (pending.TryGetValue(next, out var tail))
                {
                    int headerLen = AesGcmStreamFormat.ComputeChunkHeaderLength(tagSize);
                    byte[] headerBuf = pool.Rent(headerLen);
                    try
                    {
                        AesGcmStreamFormat.BuildChunkHeader(headerBuf.AsSpan(0, headerLen), keyId, tail.Tag, tail.DataLength, tagSize);
                        await output.WriteAsync(headerBuf.AsMemory(0, headerLen), ct).ConfigureAwait(false);
                    }
                    finally
                    {
                        pool.Return(headerBuf, clearArray: false);
                    }
                    await output.WriteAsync(tail.Data.AsMemory(0, tail.DataLength), ct).ConfigureAwait(false);
                    scope.Recycle(tail.Data);
                    pending.Remove(next);
                    next++;
                }
                foreach (var kv in pending.Values)
                {
                    scope.Recycle(kv.Data);
                }
            }, ct);
        }
    }
}

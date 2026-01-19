// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EasyExtensions.Crypto.Internals.Pipelines
{
    internal sealed class DecryptionPipeline(Stream input, Stream output,
        byte[] fileKey, uint noncePrefix, int threads, int keyId, int nonceSize,
        int tagSize, int maxChunkSize, int windowCap, long expectedTotal, bool strictLength, ArrayPool<byte> pool)
    {
        private sealed class ReorderWriter
        {
            private readonly Stream _output;
            private readonly BufferScope _scope;
            private int _window;
            private readonly int _windowCap;
            private DecryptionResult[] _ring;
            private bool[] _filled;
            private long[] _slotIndex;
            private long _nextToWrite;
            public long TotalWritten { get; private set; }

            public ReorderWriter(Stream output, BufferScope scope, int threads, int windowCap)
            {
                _output = output;
                _scope = scope;
                _windowCap = windowCap;
                const int minWindow = 4;
                _window = Math.Min(Math.Max(minWindow, threads * 4), _windowCap);
                _ring = new DecryptionResult[_window];
                _filled = new bool[_window];
                _slotIndex = new long[_window];
                _nextToWrite = 0;
                TotalWritten = 0;
            }

            private void EnsureCapacity(long neededIndex)
            {
                if (neededIndex - _nextToWrite < _window)
                {
                    return;
                }
                int newWindow = Math.Min(_window * 2, _windowCap);
                while (neededIndex - _nextToWrite >= newWindow && newWindow < _windowCap)
                {
                    newWindow = Math.Min(newWindow * 2, _windowCap);
                }
                var newRing = new DecryptionResult[newWindow];
                var newFilled = new bool[newWindow];
                var newSlotIndex = new long[newWindow];
                for (int i = 0; i < _window; i++)
                {
                    if (!_filled[i])
                    {
                        continue;
                    }
                    long idx = _slotIndex[i];
                    int newSlot = (int)(idx % newWindow);
                    newRing[newSlot] = _ring[i];
                    newFilled[newSlot] = true;
                    newSlotIndex[newSlot] = idx;
                }
                _ring = newRing; _filled = newFilled; _slotIndex = newSlotIndex; _window = newWindow;
            }

            private async Task FlushReadyAsync(CancellationToken ct)
            {
                while (true)
                {
                    int slot = (int)(_nextToWrite % _window);
                    if (_filled[slot] && _slotIndex[slot] == _nextToWrite)
                    {
                        var res = _ring[slot];
                        await _output.WriteAsync(res.Data.AsMemory(0, res.DataLength), ct).ConfigureAwait(false);
                        _scope.Recycle(res.Data);
                        TotalWritten += res.DataLength;
                        _filled[slot] = false; _nextToWrite++;
                    }
                    else break;
                }
            }

            public async Task AcceptAsync(DecryptionResult result, CancellationToken ct)
            {
                if (result.Index == _nextToWrite)
                {
                    await _output.WriteAsync(result.Data.AsMemory(0, result.DataLength), ct).ConfigureAwait(false);
                    _scope.Recycle(result.Data);
                    TotalWritten += result.DataLength;
                    _nextToWrite++;
                    await FlushReadyAsync(ct).ConfigureAwait(false);
                }
                else
                {
                    if (result.Index < _nextToWrite)
                    {
                        throw new InvalidDataException($"Duplicate chunk index detected. Received {result.Index}, next expected {_nextToWrite}.");
                    }
                    EnsureCapacity(result.Index);
                    int slot = (int)(result.Index % _window);
                    if (_filled[slot])
                    {
                        throw new InvalidDataException($"Reorder buffer slot collision. Slot {slot} already filled for index {_slotIndex[slot]}, tried to place {result.Index}.");
                    }
                    _ring[slot] = result; _slotIndex[slot] = result.Index; _filled[slot] = true;
                }
            }

            public Task FlushAsync(CancellationToken ct) => FlushReadyAsync(ct);
        }

        public async Task RunAsync(CancellationToken ct)
        {
            var jobCh = Channel.CreateBounded<DecryptionJob>(new BoundedChannelOptions(threads * 4)
            {
                SingleWriter = true,
                SingleReader = false,
                FullMode = BoundedChannelFullMode.Wait
            });
            var resCh = Channel.CreateBounded<DecryptionResult>(new BoundedChannelOptions(threads * 4)
            {
                SingleWriter = false,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait
            });

            int jobCap = threads * 4;
            int resCap = threads * 4;
            // Add extra headroom for out-of-order accumulation under contention
            int allowedBacklog = Math.Min(windowCap * 2, 8192);
            int maxCount = jobCap + threads + resCap + allowedBacklog;
            long maxBytes = (long)maxChunkSize * maxCount * 4;
            using var scope = new BufferScope(pool, maxCount: maxCount, maxBytes: maxBytes);

            var producer = ProduceAsync(jobCh.Writer, scope, ct);
            var workers = StartWorkersAsync(jobCh.Reader, resCh.Writer, scope, ct);
            var consumerTask = ConsumeAsync(resCh.Reader, scope, ct);

            long written = 0;
            try
            {
                await producer.ConfigureAwait(false);
                await Task.WhenAll(workers).ConfigureAwait(false);
            }
            finally
            {
                resCh.Writer.TryComplete();
                written = await consumerTask.ConfigureAwait(false);
            }

            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }
            if (strictLength && expectedTotal > 0 && written != expectedTotal)
            {
                throw new InvalidDataException($"Decrypted length mismatch. Expected: {expectedTotal}, Actual: {written}");
            }
        }

        private Task ProduceAsync(ChannelWriter<DecryptionJob> writer, BufferScope scope, CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                long idx = 0;
                try
                {
                    while (true)
                    {
                        ct.ThrowIfCancellationRequested();
                        if (input.CanSeek)
                        {
                            long bytesRemaining = input.Length - input.Position;
                            int minHeader = 4 + 4 + 8 + 4 + tagSize;
                            if (bytesRemaining == 0) break;
                            if (bytesRemaining < minHeader)
                            {
                                throw new EndOfStreamException("Unexpected end of stream while reading chunk header.");
                            }
                        }
                        ChunkHeader chunkHeader;
                        try
                        {
                            chunkHeader = await AesGcmStreamFormat
                                .ReadChunkHeaderAsync(input, tagSize, ct)
                                .ConfigureAwait(false);
                        }
                        catch (EndOfStreamException)
                        {
                            break;
                        }

                        if (chunkHeader.KeyId != keyId)
                        {
                            throw new InvalidDataException("Chunk key ID does not match file key ID.");
                        }
                        if (chunkHeader.PlaintextLength <= 0 || chunkHeader.PlaintextLength > maxChunkSize)
                        {
                            throw new InvalidDataException("Invalid chunk length in header.");
                        }
                        if (input.CanSeek)
                        {
                            long remaining = input.Length - input.Position;
                            if (remaining < chunkHeader.PlaintextLength)
                            {
                                throw new EndOfStreamException("Unexpected end of stream while reading chunk ciphertext.");
                            }
                        }

                        int cipherLength = (int)chunkHeader.PlaintextLength;
                        byte[] cipher = scope.Rent(cipherLength);
                        await AesGcmStreamFormat
                            .ReadExactlyAsync(input, cipher, cipherLength, ct)
                            .ConfigureAwait(false);
                        if (unchecked((ulong)idx) == ulong.MaxValue)
                        {
                            scope.Recycle(cipher);
                            throw new InvalidOperationException("Maximum number of chunks per file is 2^64-1. Counter reached ulong.MaxValue.");
                        }
                        await writer
                            .WriteAsync(new DecryptionJob(idx++, chunkHeader.Tag, cipher, cipherLength), ct)
                            .ConfigureAwait(false);
                    }
                }
                finally
                {
                    writer.TryComplete();
                }
            }, ct);
        }

        private Task[] StartWorkersAsync(ChannelReader<DecryptionJob> reader,
            ChannelWriter<DecryptionResult> writer, BufferScope scope, CancellationToken ct)
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
                        byte[] plain = scope.Rent(job.DataLength);
                        try
                        {
                            AesGcmStreamFormat.ComposeNonce(nonceBuffer, noncePrefix, job.Index);
                            AesGcmStreamFormat.FillAadMutable(aad, job.Index, job.DataLength);
                            Tag128 tagCopy = job.Tag; Span<byte> tagSpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref tagCopy, 1));
                            gcm.Decrypt(nonceBuffer, job.Cipher.AsSpan(0, job.DataLength), tagSpan, plain.AsSpan(0, job.DataLength), aad);
                            await writer.WriteAsync(new DecryptionResult(job.Index, plain, job.DataLength), ct).ConfigureAwait(false);
                        }
                        catch (CryptographicException ex)
                        {
                            scope.Recycle(plain);
                            throw new AuthenticationTagMismatchException("Chunk authentication failed.", ex);
                        }
                        finally
                        {
                            scope.Recycle(job.Cipher);
                        }
                    }
                }, ct);
            }
            return tasks;
        }

        private Task<long> ConsumeAsync(ChannelReader<DecryptionResult> reader, BufferScope scope, CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                var writer = new ReorderWriter(output, scope, threads, windowCap);
                await foreach (var result in reader.ReadAllAsync(ct))
                {
                    await writer.AcceptAsync(result, ct).ConfigureAwait(false);
                }
                await writer.FlushAsync(ct).ConfigureAwait(false);
                return writer.TotalWritten;
            }, ct);
        }
    }
}

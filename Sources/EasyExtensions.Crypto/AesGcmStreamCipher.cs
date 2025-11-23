// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

using System;
using System.IO;
using System.Buffers;
using System.Threading;
using System.IO.Pipelines;
using System.Buffers.Binary;
using System.Threading.Tasks;
using EasyExtensions.Abstractions;
using System.Security.Cryptography;
using EasyExtensions.Crypto.Internals;
using EasyExtensions.Crypto.Internals.Pipelines;

namespace EasyExtensions.Crypto
{
    /// <summary>
    /// AES-GCM streaming cipher with per-file key wrapping and per-chunk authentication.
    /// Nonce layout: 12-byte IV = 4-byte file prefix || 8-byte chunk counter.
    /// The maximum number of chunks per file is 2^64-1; exceeding this throws InvalidOperationException to avoid IV reuse.
    /// </summary>
    public class AesGcmStreamCipher : IStreamCipher
    {
        /// <summary>
        /// Represents the size, in bytes, of the authentication tag used in cryptographic operations.
        /// </summary>
        public const int TagSize = 16;

        /// <summary>
        /// Specifies the size, in bytes, of the cryptographic key.
        /// </summary>
        public const int KeySize = 32;

        /// <summary>
        /// Specifies the size, in bytes, of the nonce used for encryption operations.
        /// </summary>
        public const int NonceSize = 12;

        /// <summary>
        /// Represents the minimum allowed chunk size, in bytes, for data operations.
        /// </summary>
        public const int MinChunkSize = 8 * 1024;
        
        /// <summary>
        /// Represents the maximum allowed size, in bytes, for a single data chunk.
        /// </summary>
        /// <remarks>This constant can be used to enforce limits when processing or transmitting large
        /// data blocks to prevent excessive memory usage or to comply with protocol constraints.</remarks>
        public const int MaxChunkSize = 64 * 1024 * 1024;

        /// <summary>
        /// Represents the default chunk size, in bytes, used for data processing or transfer operations.
        /// </summary>
        /// <remarks>The value is set to 1 megabyte (1 * 1024 * 1024 bytes). This constant can be used
        /// as a standard buffer or segment size when working with large data streams or files.</remarks>
        public const int DefaultChunkSize = 1 * 1024 * 1024;

        private readonly int _keyId;
        private readonly int _windowCap;
        private readonly int _maxThreads;
        private readonly int _threadsMultiplier;
        private readonly byte[] _masterKeyBytes;
        private readonly bool _strictLengthCheck;
        private readonly RandomNumberGenerator _rng;
        private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Shared;
        private readonly int ConcurrencyLevel = Math.Min(4, Environment.ProcessorCount);

        /// <summary>
        /// Initializes a new instance of the AesGcmStreamCipher class using the specified master key and configuration
        /// options.
        /// </summary>
        /// <remarks>The concurrency level and window cap parameters allow tuning of parallelism for
        /// performance optimization. The strictLengthCheck parameter enforces input validation to help prevent
        /// cryptographic misuse. If rng is not provided, a secure default is used.</remarks>
        /// <param name="masterKey">The master key used for AES-GCM encryption and decryption. Must be exactly the required key size in bytes.</param>
        /// <param name="keyId">The identifier for the key material. Must be a positive integer. Used to distinguish between different keys
        /// in multi-key scenarios.</param>
        /// <param name="threads">The number of threads to use for parallel operations. Must be between 1 and the maximum allowed for the
        /// current environment. If null, the concurrency level is determined automatically.</param>
        /// <param name="threadsLimitMultiplier">The multiplier applied to the processor count to determine the maximum allowed concurrency level. Must be
        /// greater than or equal to 1.</param>
        /// <param name="windowCap">The maximum number of concurrent windows (or segments) processed in parallel. Must be at least 4.</param>
        /// <param name="strictLengthCheck">true to enforce strict length checking on input data; otherwise, false.</param>
        /// <param name="rng">The random number generator to use for cryptographic operations. If null, a default secure random number
        /// generator is used.</param>
        /// <exception cref="ArgumentException">Thrown if masterKey is not the required length.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if keyId is not positive, threadsLimitMultiplier is less than 1, windowCap is less than 4, or threads
        /// is outside the allowed range.</exception>
        public AesGcmStreamCipher(ReadOnlyMemory<byte> masterKey, int keyId = 1, int? threads = null, int threadsLimitMultiplier = 2, int windowCap = 1024, bool strictLengthCheck = true, RandomNumberGenerator? rng = null)
        {
            if (masterKey.Length != KeySize)
            {
                throw new ArgumentException($"Master key must be {KeySize} bytes ({KeySize * 8} bits) long.", nameof(masterKey));
            }
            if (keyId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(keyId), "Key ID must be a positive integer.");
            }
            if (threadsLimitMultiplier < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(threadsLimitMultiplier), "Threads multiplier must be >= 1.");
            }
            if (windowCap < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(windowCap), "Window cap must be >= 4.");
            }

            _masterKeyBytes = masterKey.ToArray();
            _keyId = keyId;
            _threadsMultiplier = threadsLimitMultiplier;
            _maxThreads = Math.Max(1, Environment.ProcessorCount * _threadsMultiplier);
            _windowCap = windowCap;
            _strictLengthCheck = strictLengthCheck;
            _rng = rng ?? RandomNumberGenerator.Create();
            if (threads.HasValue)
            {
                if (threads.Value < 1 || threads.Value > _maxThreads)
                {
                    throw new ArgumentOutOfRangeException(nameof(threads), $"Threads must be between 1 and {_maxThreads} (CPU * {_threadsMultiplier}).");
                }
                ConcurrencyLevel = threads.Value;
            }
            ConcurrencyLevel = Math.Clamp(ConcurrencyLevel, 1, _maxThreads);
        }

        /// <inheritdoc/>
        public async Task EncryptAsync(Stream input, Stream output, int chunkSize = DefaultChunkSize, bool leaveInputOpen = true, bool leaveOutputOpen = true, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(output);
            if (!input.CanRead)
            {
                throw new ArgumentException("Input stream must be readable.", nameof(input));
            }
            if (!output.CanWrite)
            {
                throw new ArgumentException("Output stream must be writable.", nameof(output));
            }
            if (chunkSize < MinChunkSize || chunkSize > MaxChunkSize)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize), $"Chunk size must be between {MinChunkSize} and {MaxChunkSize} bytes.");
            }

            byte[] fileKey = BufferPool.Rent(KeySize);
            try
            {
                _rng.GetBytes(fileKey.AsSpan(0, KeySize));
                // Per-file nonce prefix (4 bytes)
                Span<byte> prefixBytes = stackalloc byte[4];
                _rng.GetBytes(prefixBytes);
                uint fileNoncePrefix = BinaryPrimitives.ReadUInt32LittleEndian(prefixBytes);

                byte[] fileKeyNonce = new byte[NonceSize];
                Tag128 fileKeyTag;
                _rng.GetBytes(fileKeyNonce);
                byte[] encryptedFileKey = new byte[KeySize];
                using (var gcm = new AesGcm(_masterKeyBytes, TagSize))
                {
                    Span<byte> tagSpan = stackalloc byte[TagSize];
                    gcm.Encrypt(fileKeyNonce, fileKey.AsSpan(0, KeySize), encryptedFileKey, tagSpan);
                    fileKeyTag = Tag128.FromSpan(tagSpan);
                }

                long totalPlaintextLength = input.CanSeek ? Math.Max(0, input.Length - input.Position) : 0;
                int headerLen = AesGcmStreamFormat.ComputeFileHeaderLength(NonceSize, TagSize, KeySize);
                byte[] headerBuf = BufferPool.Rent(headerLen);
                try
                {
                    AesGcmStreamFormat.BuildFileHeader(headerBuf.AsSpan(0, headerLen), _keyId, fileNoncePrefix, fileKeyNonce, fileKeyTag, encryptedFileKey, totalPlaintextLength, NonceSize, TagSize, KeySize);
                    await output.WriteAsync(headerBuf.AsMemory(0, headerLen), ct).ConfigureAwait(false);
                }
                finally
                {
                    BufferPool.Return(headerBuf, clearArray: false);
                }

                var enc = new EncryptionPipeline(input, output, fileKey, fileNoncePrefix, chunkSize, ConcurrencyLevel, _keyId, NonceSize, TagSize, _windowCap, BufferPool);
                await enc.RunAsync(ct).ConfigureAwait(false);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(fileKey);
                BufferPool.Return(fileKey, clearArray: false);
                if (!leaveInputOpen)
                {
                    input.Dispose();
                }
                if (!leaveOutputOpen)
                {
                    output.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public async Task DecryptAsync(Stream input, Stream output, bool leaveInputOpen = true, bool leaveOutputOpen = true, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(output);
            if (!input.CanRead)
            {
                throw new ArgumentException("Input stream must be readable.", nameof(input));
            }
            if (!output.CanWrite)
            {
                throw new ArgumentException("Output stream must be writable.", nameof(output));
            }

            FileHeader header = await AesGcmStreamFormat.ReadFileHeaderAsync(input, NonceSize, TagSize, KeySize, ct).ConfigureAwait(false);
            if (header.KeyId != _keyId)
            {
                if (!leaveInputOpen)
                {
                    input.Dispose();
                }
                if (!leaveOutputOpen)
                {
                    output.Dispose();
                }
                throw new InvalidDataException($"Key ID mismatch. Expected {_keyId}, but file has {header.KeyId}.");
            }

            byte[] fileKey = BufferPool.Rent(KeySize);
            try
            {
                using (var gcm = new AesGcm(_masterKeyBytes, TagSize))
                {
                    Span<byte> tagSpan = stackalloc byte[TagSize];
                    header.Tag.CopyTo(tagSpan);
                    gcm.Decrypt(header.Nonce, header.EncryptedKey, tagSpan, fileKey.AsSpan(0, KeySize));
                }
                var dec = new DecryptionPipeline(input, output, fileKey, header.NoncePrefix, ConcurrencyLevel, _keyId, NonceSize, TagSize, MaxChunkSize, _windowCap, header.TotalPlaintextLength, _strictLengthCheck, BufferPool);
                await dec.RunAsync(ct).ConfigureAwait(false);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(fileKey);
                BufferPool.Return(fileKey, clearArray: false);
                if (!leaveInputOpen)
                {
                    input.Dispose();
                }
                if (!leaveOutputOpen)
                {
                    output.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public Task<Stream> EncryptAsync(Stream input, int chunkSize = DefaultChunkSize, bool leaveOpen = false, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            if (!input.CanRead)
            {
                throw new ArgumentException("Input stream must be readable.", nameof(input));
            }
            if (chunkSize < MinChunkSize || chunkSize > MaxChunkSize)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize), $"Chunk size must be between {MinChunkSize} and {MaxChunkSize} bytes.");
            }

            long pauseThreshold = (long)Math.Clamp(chunkSize, MinChunkSize, MaxChunkSize) * Math.Clamp(_windowCap, 4, int.MaxValue);
            long resumeThreshold = pauseThreshold / 2;
            var pipe = new Pipe(new PipeOptions(
                pool: MemoryPool<byte>.Shared,
                readerScheduler: null,
                writerScheduler: null,
                pauseWriterThreshold: pauseThreshold,
                resumeWriterThreshold: resumeThreshold,
                minimumSegmentSize: 4096,
                useSynchronizationContext: false));

            var readerStream = pipe.Reader.AsStream(leaveOpen: leaveOpen);

            _ = Task.Run(async () =>
            {
                try
                {
                    var writerStream = pipe.Writer.AsStream(leaveOpen: true);
                    await EncryptAsync(input, writerStream, chunkSize, leaveInputOpen: leaveOpen, leaveOutputOpen: true, ct).ConfigureAwait(false);
                    await pipe.Writer.CompleteAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException oce)
                {
                    await pipe.Writer.CompleteAsync(oce).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await pipe.Writer.CompleteAsync(ex).ConfigureAwait(false);
                }
                finally
                {
                    if (!leaveOpen)
                    {
                        input.Dispose();
                    }
                }
            }, ct);

            return Task.FromResult<Stream>(readerStream);
        }

        /// <inheritdoc/>
        public Task<Stream> DecryptAsync(Stream input, bool leaveOpen = false, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            if (!input.CanRead)
            {
                throw new ArgumentException("Input stream must be readable.", nameof(input));
            }

            long perChunkGuess = DefaultChunkSize;
            long pauseThreshold = (long)Math.Clamp(perChunkGuess, MinChunkSize, MaxChunkSize) * Math.Clamp(_windowCap, 4, int.MaxValue);
            long resumeThreshold = pauseThreshold / 2;
            var pipe = new Pipe(new PipeOptions(
                pool: MemoryPool<byte>.Shared,
                readerScheduler: null,
                writerScheduler: null,
                pauseWriterThreshold: pauseThreshold,
                resumeWriterThreshold: resumeThreshold,
                minimumSegmentSize: 4096,
                useSynchronizationContext: false));

            var readerStream = pipe.Reader.AsStream(leaveOpen: leaveOpen);

            _ = Task.Run(async () =>
            {
                try
                {
                    var writerStream = pipe.Writer.AsStream(leaveOpen: true);
                    await DecryptAsync(input, writerStream, leaveInputOpen: leaveOpen, leaveOutputOpen: true, ct).ConfigureAwait(false);
                    await pipe.Writer.CompleteAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException oce)
                {
                    await pipe.Writer.CompleteAsync(oce).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await pipe.Writer.CompleteAsync(ex).ConfigureAwait(false);
                }
                finally
                {
                    if (!leaveOpen)
                    {
                        input.Dispose();
                    }
                }
            }, ct);

            return Task.FromResult<Stream>(readerStream);
        }
    }
}

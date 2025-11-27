using System;
using System.IO;
using System.Buffers;
using System.Collections.Generic;

namespace EasyExtensions.Streams
{
    /// <summary>
    /// Provides a stream wrapper that enables reading the underlying stream in fixed-size chunks.
    /// </summary>
    /// <remarks>ChunkedStream is useful for processing large streams in manageable segments, such as when
    /// uploading or processing data in parts. The class manages chunk boundaries and ensures that each chunked stream
    /// contains up to the specified chunk size. The underlying stream is disposed when ChunkedStream is
    /// disposed.</remarks>
    public class ChunkedStream : IDisposable
    {
        private readonly Stream _baseStream;
        private readonly int _chunkSize;
        private readonly bool _disposed;

        /// <summary>
        /// Initializes a new instance of the ChunkedStream class that reads from or writes to the specified base stream
        /// in fixed-size chunks.
        /// </summary>
        /// <param name="baseStream">The underlying stream to read from or write to. Cannot be null.</param>
        /// <param name="chunkSize">The size, in bytes, of each chunk to use when reading or writing. Must be greater than zero.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if chunkSize is less than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown if baseStream is null.</exception>
        public ChunkedStream(Stream baseStream, int chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be greater than zero.");
            }
            _baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            _chunkSize = chunkSize;
        }

        /// <summary>
        /// Returns an enumerable collection of streams, each containing a chunk of data read sequentially from the
        /// underlying stream.
        /// </summary>
        /// <remarks>Each returned stream must be disposed by the caller after use to release resources.
        /// The method reads from the current position of the underlying stream and continues until the end of the
        /// stream is reached. This method is not thread-safe.</remarks>
        /// <returns>An enumerable sequence of streams, where each stream contains up to the configured chunk size of data from
        /// the underlying stream. The final chunk may be smaller if the end of the stream is reached.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the ChunkedStream has been disposed.</exception>
        public IEnumerable<Stream> GetChunks()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ChunkedStream));
            }
            byte[] buffer = ArrayPool<byte>.Shared.Rent(_chunkSize);
            int bytesRead;
            while ((bytesRead = _baseStream.Read(buffer, 0, _chunkSize)) > 0)
            {
                MemoryStream chunkStream = new MemoryStream();
                chunkStream.Write(buffer, 0, bytesRead);
                chunkStream.Position = 0;
                yield return chunkStream;
            }
            ArrayPool<byte>.Shared.Return(buffer);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the class.
        /// </summary>
        /// <remarks>Call this method when you are finished using the object to release unmanaged
        /// resources and perform other cleanup operations. After calling Dispose, the object should not be
        /// used.</remarks>
        public void Dispose()
        {
            if (!_disposed)
            {
                _baseStream.Dispose();
            }
        }
    }
}

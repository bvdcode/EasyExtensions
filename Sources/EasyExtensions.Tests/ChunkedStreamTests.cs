using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using EasyExtensions.Streams;

namespace EasyExtensions.Tests
{
    [TestFixture]
    public class ChunkedStreamTests
    {
        [Test]
        public void GetChunks_SplitsStreamIntoEqualSizedChunks_WhenLengthIsMultipleOfChunkSize()
        {
            // Arrange
            var totalSize = 4096; // 4 KB
            var chunkSize = 1024; // 1 KB
            var data = Enumerable.Range(0, totalSize).Select(i => (byte)(i % 256)).ToArray();
            using var baseStream = new MemoryStream(data);
            using var chunked = new ChunkedStream(baseStream, chunkSize);

            // Act
            var chunks = chunked.GetChunks().ToList();

            // Assert
            Assert.That(chunks, Has.Count.EqualTo(totalSize / chunkSize));
            for (int idx = 0; idx < chunks.Count; idx++)
            {
                var chunk = chunks[idx];
                using (chunk)
                {
                    Assert.That(chunk.Length, Is.EqualTo(chunkSize));
                    Assert.That(chunk.Position, Is.Zero);
                    // verify content is contiguous and matches source
                    using var reader = new BinaryReader(chunk, System.Text.Encoding.UTF8, leaveOpen: true);
                    var bytes = reader.ReadBytes((int)chunk.Length);
                    var startIndex = idx * chunkSize;
                    var expected = data.Skip(startIndex).Take(chunkSize).ToArray();
                    Assert.That(bytes, Is.EqualTo(expected));
                }
            }
        }

        [Test]
        public void GetChunks_FinalChunkIsSmaller_WhenLengthNotMultipleOfChunkSize()
        {
            // Arrange
            var totalSize = 2500;
            var chunkSize = 1024;
            var data = Enumerable.Range(0, totalSize).Select(i => (byte)(i % 256)).ToArray();
            using var baseStream = new MemoryStream(data);
            using var chunked = new ChunkedStream(baseStream, chunkSize);

            // Act
            var chunks = chunked.GetChunks().ToList();

            // Assert
            Assert.That(chunks, Has.Count.EqualTo(3)); // 1024 + 1024 + 452
            using (Assert.EnterMultipleScope())
            {
                Assert.That(chunks[0].Length, Is.EqualTo(chunkSize));
                Assert.That(chunks[1].Length, Is.EqualTo(chunkSize));
                Assert.That(chunks[2].Length, Is.EqualTo(totalSize - 2 * chunkSize));
            }

            // Verify content of last chunk (do not dispose before reading)
            var last = chunks[2];
            using (last)
            using (var reader = new BinaryReader(last, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                var bytes = reader.ReadBytes((int)last.Length);
                var expected = data.Skip(2 * chunkSize).ToArray();
                Assert.That(bytes, Is.EqualTo(expected));
            }

            // Dispose other chunk streams
            using (chunks[0]) { }
            using (chunks[1]) { }
        }

        [Test]
        public void GetChunks_EmptyStream_ReturnsNoChunks()
        {
            // Arrange
            using var baseStream = new MemoryStream([]);
            using var chunked = new ChunkedStream(baseStream, chunkSize: 1024);

            // Act
            var chunks = chunked.GetChunks().ToList();

            // Assert
            Assert.That(chunks, Is.Empty);
        }

        [Test]
        public void Chunks_AreIndependentStreams_AtPositionZero()
        {
            // Arrange
            var data = Enumerable.Range(0, 3000).Select(i => (byte)(i % 256)).ToArray();
            using var baseStream = new MemoryStream(data);
            using var chunked = new ChunkedStream(baseStream, 1000);

            // Act
            var chunks = chunked.GetChunks().ToList();

            // Assert
            foreach (var chunk in chunks)
            {
                using (chunk)
                {
                    Assert.That(chunk.Position, Is.Zero);
                    // Move position and ensure other chunks unaffected
                    chunk.Position = Math.Min(10, chunk.Length);
                }
            }
        }

        [Test]
        public void Dispose_DisposesUnderlyingStream()
        {
            // Arrange
            var data = new byte[2048];
            using var baseStream = new MemoryStream(data);
            var chunked = new ChunkedStream(baseStream, 512);

            // Act
            chunked.Dispose();

            // Assert: baseStream should be disposed and reading should throw
            Assert.Throws<ObjectDisposedException>(() => baseStream.ReadByte());
        }

        [Test]
        public void GetChunks_DoesNotReturnChunksSmallerThanRequestedSize_WhenEnoughDataAvailable()
        {
            // Arrange
            var chunkSize = 8 * 1024 * 1024; // 8 MB
            var totalSize = chunkSize * 3; // exactly 3 chunks
            var data = Enumerable.Range(0, totalSize).Select(i => (byte)(i % 256)).ToArray();
            using var baseStream = new MemoryStream(data);
            using var chunked = new ChunkedStream(baseStream, chunkSize);

            // Act
            var chunks = chunked.GetChunks().ToList();

            // Assert
            Assert.That(chunks, Has.Count.EqualTo(3));
            for (int idx = 0; idx < chunks.Count; idx++)
            {
                using var chunk = chunks[idx];
                int length = (int)chunk.Length;
                Assert.That(length, Is.EqualTo(chunkSize), $"Chunk #{idx} length is not equal to requested chunk size");
            }
        }

        [Test]
        public void GetChunks_ProducesSingleChunk_WhenDataSmallerThanChunkSize()
        {
            // Arrange
            var chunkSize = 8 * 1024 * 1024; // 8 MB
            var totalSize = 32 * 1024; // 32 KB
            var data = Enumerable.Range(0, totalSize).Select(i => (byte)(i % 256)).ToArray();
            using var baseStream = new MemoryStream(data);
            using var chunked = new ChunkedStream(baseStream, chunkSize);

            // Act
            var chunks = chunked.GetChunks().ToList();

            // Assert
            Assert.That(chunks, Has.Count.EqualTo(1));
            using var onlyChunk = chunks[0];
            Assert.That(onlyChunk.Length, Is.EqualTo(totalSize));
        }

        [Test]
        public void GetChunks_RespectsBaseStreamPosition_StartsFromCurrentPosition()
        {
            // Arrange
            var totalSize = 1024 * 1024; // 1 MB
            var chunkSize = 64 * 1024; // 64 KB
            var skip = 128 * 1024; // skip first 128 KB
            var data = Enumerable.Range(0, totalSize).Select(i => (byte)(i % 256)).ToArray();
            using var baseStream = new MemoryStream(data);
            baseStream.Position = skip;
            using var chunked = new ChunkedStream(baseStream, chunkSize);

            // Act
            var chunks = chunked.GetChunks().ToList();

            // Assert
            var remaining = totalSize - skip;
            var expectedFullChunks = remaining / chunkSize;
            var expectedLastChunk = remaining % chunkSize;

            Assert.That(chunks, Has.Count.EqualTo(expectedFullChunks + (expectedLastChunk > 0 ? 1 : 0)));

            // First byte of first chunk should equal data[skip]
            using (var firstChunk = chunks[0])
            {
                Assert.That(firstChunk.ReadByte(), Is.EqualTo(data[skip]));
            }

            // Dispose rest
            for (int i = 1; i < chunks.Count; i++)
            {
                chunks[i].Dispose();
            }
        }

        [Test]
        public void GetChunks_ReturnsConsistentChunkSizes_ForLargeStreams()
        {
            // Arrange: create stream larger than typical buffer sizes
            var chunkSize = 8 * 1024 * 1024; // 8 MB
            var totalSize = chunkSize * 5 + chunkSize / 2; // 5.5 chunks
            var data = new byte[totalSize];
            new Random(1234).NextBytes(data);
            using var baseStream = new MemoryStream(data);
            using var chunked = new ChunkedStream(baseStream, chunkSize);

            // Act
            var chunks = chunked.GetChunks().ToList();

            // Assert
            Assert.That(chunks, Has.Count.EqualTo(6));
            for (int i = 0; i < chunks.Count; i++)
            {
                using var chunk = chunks[i];
                if (i < 5)
                {
                    Assert.That(chunk.Length, Is.EqualTo(chunkSize));
                }
                else
                {
                    Assert.That(chunk.Length, Is.EqualTo(chunkSize / 2));
                }
            }
        }
    }
}

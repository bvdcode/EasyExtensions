// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Crypto.Tests.TestUtils
{
    // A seekable stream that reports a given Length but returns EOF immediately (no data).
    // Useful to validate header fields for huge files without generating payload.
    internal sealed class SeekableSyntheticReadStream(long length) : Stream
    {
        private readonly long _length = length;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _length;
        public override long Position { get; set; }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => 0; // immediate EOF
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(0);
        public override long Seek(long offset, SeekOrigin origin)
        {
            Position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => _length + offset,
                _ => Position
            };
            return Position;
        }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}

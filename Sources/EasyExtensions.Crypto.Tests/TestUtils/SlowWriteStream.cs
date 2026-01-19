// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Crypto.Tests.TestUtils
{
    internal class SlowWriteStream(Stream inner, int delayMs) : Stream
    {
        public Stream Inner { get; } = inner ?? throw new ArgumentNullException(nameof(inner));

        public override bool CanRead => false;
        public override bool CanSeek => Inner.CanSeek;
        public override bool CanWrite => true;
        public override long Length => Inner.Length;
        public override long Position { get => Inner.Position; set => Inner.Position = value; }
        public override void Flush() => Inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => Inner.Seek(offset, origin);
        public override void SetLength(long value) => Inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count)
        {
            Thread.Sleep(delayMs);
            Inner.Write(buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
            await Inner.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
            await Inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
    }
}

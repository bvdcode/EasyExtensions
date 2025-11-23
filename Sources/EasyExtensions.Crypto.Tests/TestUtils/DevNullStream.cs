// SPDX-License-Identifier: AGPL-3.0-only
// Copyright (c) 2025 Vadim Belov

namespace EasyExtensions.Crypto.Tests.TestUtils
{
    internal class DevNullStream : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position { get => 0; set { } }
        public override void Flush() { }
        public override int Read(byte[] b, int o, int c) => 0;
        public override long Seek(long o, SeekOrigin s) => 0;
        public override void SetLength(long v) { }
        public override void Write(byte[] b, int o, int c) { }
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> b, CancellationToken t = default)
            => ValueTask.CompletedTask;
    }
}

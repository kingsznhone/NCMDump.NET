namespace NCMDump.Core
{
    public class RC4_NCM_Stream : Stream, IRC4_NCM_Stream
    {
        private readonly Stream _innerStream;
        private readonly RC4_NCM rc4;

        public RC4_NCM_Stream(Stream innerStream, byte[] key)
        {
            this._innerStream = innerStream;
            rc4 = new RC4_NCM(key);
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;

        public override long Position
        {
            get => _innerStream.Position;
            set => _innerStream.Position = value;
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = Read(buffer.AsSpan(offset, count));
            return bytesRead;
        }

        public override int Read(Span<byte> buffer)
        {
            int bytesRead = _innerStream.Read(buffer);
            rc4.Encrypt(buffer);
            return bytesRead;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return new Task<int>(() => Read(buffer.AsSpan(offset, count)));
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int bytesRead = _innerStream.Read(buffer.Span);
            return new ValueTask<int>(rc4.Encrypt(buffer));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override bool CanTimeout => base.CanTimeout;

        public override void Close() => base.Close();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override void Write(ReadOnlySpan<byte> buffer)
            => throw new NotSupportedException();

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public override void CopyTo(Stream destination, int bufferSize)
            => throw new NotSupportedException();
    }
}
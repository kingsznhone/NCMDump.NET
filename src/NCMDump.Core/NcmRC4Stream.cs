namespace NCMDump.Core
{
    public class NcmRC4Stream : Stream, IRC4_NCM_Stream
    {
        private readonly Stream _innerStream;
        private readonly NcmRC4 _rc4;

        public NcmRC4Stream(Stream innerStream, byte[] key)
        {
            _innerStream = innerStream;
            _rc4 = new NcmRC4(key);
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        public override bool CanTimeout => _innerStream.CanTimeout;

        public override long Position
        {
            get => _innerStream.Position;
            set => _innerStream.Position = value;
        }

        public override void Flush() => _innerStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer.AsSpan(offset, count));
        }

        public override int Read(Span<byte> buffer)
        {
            int bytesRead = _innerStream.Read(buffer);
            if (bytesRead > 0)
            {
                _rc4.Transform(buffer[..bytesRead]);
            }
            return bytesRead;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int bytesRead = await _innerStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

            if (bytesRead > 0)
            {
                await Task.Run(() => _rc4.Transform(buffer[..bytesRead]), cancellationToken).ConfigureAwait(false);
            }
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);

        public override void SetLength(long value) => _innerStream.SetLength(value);

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
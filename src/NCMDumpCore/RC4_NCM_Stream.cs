namespace NCMDumpCore
{
    public class RC4_NCM_Stream : Stream
    {
        private readonly Stream innerStream;
        private readonly RC4_NCM rc4;

        public RC4_NCM_Stream(Stream innerStream, byte[] key)
        {
            this.innerStream = innerStream;
            rc4 = new RC4_NCM(key);
        }

        public override bool CanRead => innerStream.CanRead;
        public override bool CanSeek => innerStream.CanSeek;
        public override bool CanWrite => innerStream.CanWrite;
        public override long Length => innerStream.Length;

        public override long Position
        {
            get => innerStream.Position;
            set => innerStream.Position = value;
        }

        public override void Flush()
        {
            innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Span<byte> span = new byte[count];
            innerStream.Seek(offset, SeekOrigin.Current);
            int bytesRead = Read(span);
            span.CopyTo(buffer);

            //int bytesRead = innerStream.Read(buffer, offset, count);
            //byte[] decryptedBytes = rc4.Encrypt(buffer[offset..(offset + bytesRead)]);
            //Array.Copy(decryptedBytes, 0, buffer, offset, decryptedBytes.Length);
            return bytesRead;
        }

        public override int Read(Span<byte> buffer)
        {
            int bytesRead = innerStream.Read(buffer);
            rc4.Encrypt(ref buffer);
            return bytesRead;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            innerStream.Seek(offset, SeekOrigin.Current);
            int bytesRead = await Task.Run(() => Read(buffer));
            return bytesRead;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int bytesRead = innerStream.Read(buffer.Span);
            await Task.Run(() => rc4.Encrypt(buffer));
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Span<byte> span = buffer.AsSpan(offset, count);
            rc4.Encrypt(ref span);
            innerStream.Write(span);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            Span<byte> span = new byte[buffer.Length];
            buffer.CopyTo(span);
            rc4.Encrypt(ref span);

            innerStream.Write(buffer);
        }

        public override async Task WriteAsync(byte[] data, int offset, int count, CancellationToken cancellationToken)
        {
            byte[] buffer = data[offset..(offset + count)];
            byte[] encrypted = await Task.Run(() => rc4.Encrypt(buffer));
            await innerStream.WriteAsync(encrypted, cancellationToken);
            return;
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = data.ToArray();
            await Task.Run(() => rc4.Encrypt(buffer));
            await innerStream.WriteAsync(buffer, cancellationToken);
            return;
        }
    }
}
namespace NCMDumpCore
{
    internal class RC4_NCM_Stream : Stream
    {
        private Stream innerStream;
        private RC4_NCM rc4;

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
            Span<byte> span = buffer[offset..(offset + count)];

            int bytesWrite = rc4.Encrypt(ref span);

            innerStream.Write(span);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            Span<byte> span = new byte[buffer.Length];
            buffer.CopyTo(span);
            int bytesWrite = rc4.Encrypt(ref span);

            innerStream.Write(buffer);
        }
    }
}
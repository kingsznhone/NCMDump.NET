namespace NCMDump.Core
{
    public interface IRC4Stream : IDisposable
    {
        public bool CanRead { get; }
        public bool CanSeek { get; }
        public bool CanWrite { get; }
        public long Length { get; }
        public long Position { get; set; }

        public long Seek(long offset, SeekOrigin origin);

        public int Read(byte[] buffer, int offset, int count);

        public int Read(Span<byte> buffer);

        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
    }
}
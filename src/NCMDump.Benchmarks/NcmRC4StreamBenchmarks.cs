using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using NCMDump.Core;
using System.Runtime.CompilerServices;

namespace NCMDump.Benchmarks;

/// <summary>
/// Measures streaming reads through <see cref="NcmRC4Stream"/> using the framework's default copy
/// buffer size, and compares them with an unencrypted <see cref="MemoryStream"/> (baseline).
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net11_0)]
public class NcmRC4StreamBenchmarks
{
    public const int PayloadBytes = 64 * 1024 * 1024;

    private const int KeyLength = 256;
    private const int CopyBufferSize = 80 * 1024;

    private byte[] _payload = [];
    private byte[] _key = [];
    private byte[] _copyBuffer = [];
    private SinkStream _sink = null!;

    [GlobalSetup]
    public void Setup()
    {
        _payload = new byte[PayloadBytes];
        new Random(20240101).NextBytes(_payload);

        _key = new byte[KeyLength];
        new Random(42).NextBytes(_key);

        _copyBuffer = new byte[CopyBufferSize];
        _sink = new SinkStream();
    }

    [Benchmark]
    public long NcmRC4Stream_CopyTo()
    {
        using var source = new MemoryStream(_payload, writable: false);
        using var stream = new NcmRC4Stream(source, _key);

        stream.CopyTo(_sink);

        return _sink.Observed;
    }

    [Benchmark]
    public async Task<long> NcmRC4Stream_CopyToAsync()
    {
        using var source = new MemoryStream(_payload, writable: false);
        await using var stream = new NcmRC4Stream(source, _key);

        await stream.CopyToAsync(_sink);

        return _sink.Observed;
    }

    /// <summary>
    /// Plain copy reference. Uses an explicit read loop because the JIT can eliminate the memcpy
    /// performed by <see cref="Stream.CopyTo(Stream)"/> once tier-1 compilation kicks in.
    /// </summary>
    [Benchmark(Baseline = true)]
    public long PlainStream_Copy_Baseline()
    {
        using var source = new MemoryStream(_payload, writable: false);

        int read;
        while ((read = source.Read(_copyBuffer, 0, _copyBuffer.Length)) != 0)
        {
            _sink.Write(_copyBuffer, 0, read);
        }

        return _sink.Observed;
    }

    /// <summary>
    /// Consumes the written bytes so the JIT cannot eliminate the copy when the destination is
    /// otherwise unobservable (as it would be with <see cref="Stream.Null"/>).
    /// </summary>
    private sealed class SinkStream : Stream
    {
        private long _bytes;
        private byte _last;

        public long Observed => _bytes ^ _last;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            _bytes += buffer.Length;
            if (!buffer.IsEmpty)
            {
                _last = (byte)(buffer[0] ^ buffer[buffer.Length / 2] ^ buffer[^1]);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
            => Write(buffer.AsSpan(offset, count));

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Write(buffer.AsSpan(offset, count));
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Write(buffer.Span);
            return default;
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using NCMDump.Core;
using System;

namespace NCMDump.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net90)]
    public class RC4Benchmark
    {
        private NcmRC4 _ncmRC4 = null!;
        private NcmRC4Native _ncmRC4_Native = null!;
        private byte[] _dataSource = null!;
        private byte[] _dataCopy = null!; 
        private readonly Random _random = new Random(42); 

        [GlobalSetup]
        public void Setup()
        {
            byte[] key = new byte[32];
            _random.NextBytes(key);
            _ncmRC4 = new NcmRC4(key);
            _ncmRC4_Native = new NcmRC4Native(key);


            _dataSource = new byte[64 * 1024 * 1024]; 
            _random.NextBytes(_dataSource);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _dataCopy = new byte[_dataSource.Length];
            Array.Copy(_dataSource, 0, _dataCopy, 0, _dataSource.Length);
        }

        [Benchmark(Description = "Transform", Baseline = true)]
        public int Transform()
        {
            return _ncmRC4.Transform(_dataCopy.AsSpan());
        }

        [Benchmark(Description = "TransformNative")]
        public int Transform_Native()
        {
            return _ncmRC4_Native.Transform(_dataCopy.AsSpan());
        }
    }
}
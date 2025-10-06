using BenchmarkDotNet.Running;

namespace NCMDump.Benchmark
{
    internal class Program
    {
        private static void Main(string[] args)
        {
#if DEBUG
            BenchmarkRunner.Run<RC4Benchmark>(new DebugBuildConfig());
#else
            BenchmarkRunner.Run<RC4Benchmark>();
#endif
        }
    }
}
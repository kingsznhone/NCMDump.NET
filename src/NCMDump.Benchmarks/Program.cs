using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using NCMDump.Benchmarks;

var config = DefaultConfig.Instance
    .AddColumn(new ThroughputColumn(NcmRC4StreamBenchmarks.PayloadBytes));

BenchmarkRunner.Run<NcmRC4StreamBenchmarks>(config, args);

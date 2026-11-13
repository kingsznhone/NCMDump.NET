using System.Globalization;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace NCMDump.Benchmarks;

/// <summary>
/// Adds a MiB/s column derived from the mean time of a benchmark that reads the whole payload once.
/// </summary>
public sealed class ThroughputColumn(long bytesPerOperation) : IColumn
{
    public string Id => "MiB/s";
    public string ColumnName => "MiB/s";
    public string Legend => "Payload throughput";
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Custom;
    public int PriorityInCategory => 0;
    public bool IsNumeric => true;
    public UnitType UnitType => UnitType.Dimensionless;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        => GetValue(summary, benchmarkCase, summary.Style);

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
    {
        var statistics = summary[benchmarkCase]?.ResultStatistics;
        if (statistics is null || double.IsNaN(statistics.Mean) || statistics.Mean <= 0)
        {
            return "N/A";
        }

        double mebibytesPerSecond = bytesPerOperation / (statistics.Mean / 1e9) / (1024 * 1024);
        return mebibytesPerSecond.ToString("N1", style.CultureInfo);
    }

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

    public bool IsAvailable(Summary summary) => true;
}

using NCMDump.Core;
using System.CommandLine;

namespace NCMDump.CLI;

internal static class Application
{
    private static readonly NCMDumper Dumper = new();

    internal static async Task<int> Main(string[] args)
    {
        var pathsArgument = new Argument<string[]>("paths")
        {
            Description = "One or more .ncm files or directories to convert.",
            Arity = ArgumentArity.ZeroOrMore
        };

        var depthOption = new Option<int>("--depth", "-d")
        {
            Description = "Maximum directory recursion depth.",
            DefaultValueFactory = _ => 16
        };
        depthOption.Validators.Add(result =>
        {
            if (result.GetValueOrDefault<int>() < 1)
                result.AddError("Depth must be at least 1.");
        });

        var outputOption = new Option<DirectoryInfo?>("--output", "-o")
        {
            Description = "Output directory for converted files. Defaults to the same directory as each source file."
        };

        var rootCommand = new RootCommand("NCMDump CLI — converts NetEase Cloud Music .ncm files to playable audio.")
        {
            pathsArgument,
            depthOption,
            outputOption
        };

        if (OperatingSystem.IsWindows())
            rootCommand.Description +=
                "\n\nTip: you can also drag .ncm files or a folder onto ncmdump.exe to start conversion.";

        rootCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            string[] paths = parseResult.GetValue(pathsArgument) ?? [];
            int maxDepth = parseResult.GetValue(depthOption);
            DirectoryInfo? outputDir = parseResult.GetValue(outputOption);

            if (paths.Length == 0)
            {
                Console.Error.WriteLine(
                    "No input specified. Use --help for usage information.");
                return 1;
            }

            int failures = 0;

            foreach (string path in paths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (Directory.Exists(path))
                {
                    failures += await WalkThroughAsync(
                        new DirectoryInfo(path), outputDir, maxDepth, 0, cancellationToken);
                }
                else if (File.Exists(path))
                {
                    failures += await DumpFileAsync(new FileInfo(path), outputDir, cancellationToken);
                }
                else
                {
                    Console.Error.WriteLine($"Not found: {path}");
                    failures++;
                }
            }

            return failures == 0 ? 0 : 1;
        });

        return await rootCommand.Parse(args).InvokeAsync();
    }

    /// <summary>Converts a single .ncm file, writing output to <paramref name="outputDir"/> when specified.</summary>
    private static async Task<int> DumpFileAsync(
        FileInfo file, DirectoryInfo? outputDir, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string sourcePath = file.FullName;
        string effectivePath = sourcePath;

        if (outputDir is not null)
        {
            outputDir.Create();
            string destName = Path.GetFileNameWithoutExtension(file.Name);
            // Use a temporary copy in the output dir so NCMDumper writes the result there.
            // NCMDumper derives output path from input path, so we pass a relocated copy path.
            effectivePath = Path.Combine(outputDir.FullName, file.Name);
            File.Copy(sourcePath, effectivePath, overwrite: true);
        }

        Console.Write($"Dumping: {sourcePath} ...... ");
        bool ok = await Dumper.ConvertAsync(effectivePath);
        Console.WriteLine(ok ? "OK" : "Fail");

        // Remove the temporary .ncm copy from the output dir (the converted file stays).
        if (outputDir is not null && File.Exists(effectivePath))
            File.Delete(effectivePath);

        return ok ? 0 : 1;
    }

    /// <summary>Recursively walks a directory and converts all .ncm files found.</summary>
    private static async Task<int> WalkThroughAsync(
        DirectoryInfo dir, DirectoryInfo? outputDir, int maxDepth, int currentDepth,
        CancellationToken cancellationToken)
    {
        if (currentDepth >= maxDepth)
            return 0;

        Console.WriteLine($"DIR: {dir.FullName}");
        int failures = 0;

        foreach (DirectoryInfo sub in dir.GetDirectories())
        {
            cancellationToken.ThrowIfCancellationRequested();
            failures += await WalkThroughAsync(sub, outputDir, maxDepth, currentDepth + 1, cancellationToken);
        }

        foreach (FileInfo file in dir.EnumerateFiles("*.ncm"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            failures += await DumpFileAsync(file, outputDir, cancellationToken);
        }

        Console.WriteLine();
        return failures;
    }
}
using System.Reflection;
using NCMDump.Core;

namespace NCMDump.CLI
{
    public class Application
    {
        public static NCMDumper Dumper = new();
        public static int DirDepth = 0;

        public static void Main(params string[] args)
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version;

            if (args.Length == 0)
            {
                Console.WriteLine("==============================");
                Console.WriteLine($"NCMDump CLI - Version {version}");
                Console.WriteLine("==============================");
                Console.WriteLine("");
                if (OperatingSystem.IsWindows())
                {
                    Console.WriteLine("./ncmdump.exe <file_or_dir1> [<file_or_dir2> ... <file_or_dirN>]");
                    Console.WriteLine("Or drag [*.ncm] file or directory on exe to start...");
                }
                if (OperatingSystem.IsLinux())
                {
                    Console.WriteLine("./ncmdump <file_or_dir1> [<file_or_dir2> ... <file_or_dirN>]");
                }
                return;
            }

            try
            {
                foreach (string path in args)
                {
                    if (new DirectoryInfo(path).Exists)
                    {
                        WalkThrough(new DirectoryInfo(path));
                    }
                    else if (new FileInfo(path).Exists)
                    {
                        Console.Write($"Dumping: {new FileInfo(path).FullName} ......");
                        if (Dumper.Convert(path)) Console.WriteLine("OK");
                        else Console.WriteLine("Fail");
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.Write("Press Enter to Exit...");
            Console.ReadLine();
            return;

            void WalkThrough(DirectoryInfo dir)
            {
                DirDepth++;
                if (DirDepth > 16)
                {
                    DirDepth--;
                    return;
                }
                Console.WriteLine("DIR: " + dir.FullName);
                foreach (DirectoryInfo d in dir.GetDirectories())
                {
                    WalkThrough(d);
                }
                foreach (FileInfo f in dir.EnumerateFiles("*.ncm"))
                {
                    Console.Write($"Dumping: {f.FullName} ......");
                    if (Dumper.Convert(f.FullName)) Console.WriteLine("OK");
                    else Console.WriteLine("...Fail");
                }

                Console.WriteLine();
                DirDepth--;
            }
        }
    }
}
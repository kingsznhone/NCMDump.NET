﻿using NCMDumpCore;

public class NCMDumpCLI
{
    public static void Main(params string[] args)
    {
        NCMDumper Core = new();

        if (args.Length == 0)
        {
            Console.WriteLine("Drag [*.ncm] file or directory on exe to start");
        }
        else
        {
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
                        Console.WriteLine("FILE: " + new FileInfo(path).FullName);
                        if (Core.Convert(path)) Console.WriteLine("... Convert OK");
                        else Console.WriteLine("... Convert Fail");
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        Console.Write("Press Any Key to Exit...");
        Console.ReadLine();
        return;

        void WalkThrough(DirectoryInfo dir)
        {
            Console.WriteLine("DIR: " + dir.FullName);
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                WalkThrough(d);
            }
            foreach (FileInfo f in dir.EnumerateFiles())
            {
                Console.WriteLine("Converting : " + f.FullName);
                if (Core.Convert(f.FullName)) Console.WriteLine("...OK");
                else Console.WriteLine("...Fail");
                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
}
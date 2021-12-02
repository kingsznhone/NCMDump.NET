// See https://aka.ms/new-console-template for more information
using NCMDumpCore;

public class NCMDumpCLI
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Drag [*.ncm] file on exe to start");
        }
        else
        {
            NCMDump Core = new NCMDump();
            foreach (string path in args)
            {
                Console.WriteLine(path);
                if (Core.Convert(path)) Console.WriteLine("...OK");
                else  Console.WriteLine("...Fail");
            } 
        }

        Console.Write("Press Any Key to Exit...");
        Console.ReadLine();
    }
}






using System;
using libppexport;

namespace ppimgdiff
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ppimgdiff.exe PpFilePath OutputFolderPath");
                return;
            }
            var ppFilePath = args[0];
            var outputFolderPath = args[1];

            PowerPointExporter.ExportAsPng(ppFilePath, outputFolderPath);
        }
    }
}

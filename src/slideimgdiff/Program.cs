using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using libppexport;

namespace ppimgdiff
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ppimgdiff.exe PpFilePath1 PpFilePath2");
                return;
            }
            var pathCtx1 = new PathContext(args[0]);
            var pathCtx2 = new PathContext(args[1]);

            pathCtx1.CreateWorkFolders();
            pathCtx2.CreateWorkFolders();

            // Export the PowerPoint slides as PNG image file.
            const string ExportFileNamePattern = "s{0:0000}.png";
            Task.WaitAll(new Task[] {
                Task.Run(() => { PowerPointExporter.ExportAsPng(pathCtx1.InputFilePath, pathCtx1.SourceImageFolderPath, ExportFileNamePattern); }),
                Task.Run(() => { PowerPointExporter.ExportAsPng(pathCtx2.InputFilePath, pathCtx2.SourceImageFolderPath, ExportFileNamePattern); })
            });

            // Create the diff drawn added images.
            var sourceImagePathPairs = CreateFilePathPairs(pathCtx1.SourceImageFolderPath, pathCtx2.SourceImageFolderPath, "s????.png");
            foreach (var sourceImagePathPair in sourceImagePathPairs)
            {
                var resultImagePathPair = CreateFolderReplacedNewPathPair(sourceImagePathPair, new PathPair(pathCtx1.ResultImageFolderPath, pathCtx2.ResultImageFolderPath));
                const double sourceImageWeight = 0.6;
                var options = new DiffDrawnImageOptions()
                {
                    BlurSize = new Size(5, 5),
                    FillColor = Color.FromArgb(0, 255, 0),
                };
                DiffDrawnAddedImageMaker.SaveDiffDrawnAddedImage(sourceImagePathPair.Path1, resultImagePathPair.Path1, sourceImagePathPair.Path2, resultImagePathPair.Path2, sourceImageWeight, options);
            }
        }

        private static PathPair CreateFolderReplacedNewPathPair(PathPair pathPair, PathPair replacedFolderPathPair)
        {
            return new PathPair(
                Path.Combine(replacedFolderPathPair.Path1, Path.GetFileName(pathPair.Path1)),
                Path.Combine(replacedFolderPathPair.Path2, Path.GetFileName(pathPair.Path2))
            );
        }

        private static PathPair[] CreateFilePathPairs(string folderPath1, string folderPath2, string searchPattern)
        {
            var enumOptions = new EnumerationOptions() {
                RecurseSubdirectories = false,
                IgnoreInaccessible = true,
                ReturnSpecialDirectories = false,
                MatchCasing = MatchCasing.CaseInsensitive,
                MatchType = MatchType.Simple,
            };
            var filesInFolder1 = new List<string>(Directory.GetFiles(folderPath1, searchPattern, enumOptions));
            var filesInFolder2 = new List<string>(Directory.GetFiles(folderPath2, searchPattern, enumOptions));
            filesInFolder1.Sort();
            filesInFolder2.Sort();

            var pairRange = filesInFolder1.Count < filesInFolder2.Count ? filesInFolder1.Count : filesInFolder2.Count;
            var pathPairs = new PathPair[pairRange];
            for (int i = 0; i < pathPairs.Length; i++)
            {
                pathPairs[i] = new PathPair(filesInFolder1[i], filesInFolder2[i]);
            }

            return pathPairs;
        }
    }
}

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
            var ppFilePath1 = args[0];
            var ppFilePath2 = args[1];

            // Export the PowerPoint slides as PNG file.
            var exportFolderPath1 = CreatePngExportFolder(ppFilePath1);
            var exportFolderPath2 = CreatePngExportFolder(ppFilePath2);
            var exportTasks = new Task[] {
                Task.Run(() => { PowerPointExporter.ExportAsPng(ppFilePath1, exportFolderPath1, "s{0:0000}.png"); }),
                Task.Run(() => { PowerPointExporter.ExportAsPng(ppFilePath2, exportFolderPath2, "s{0:0000}.png"); })
            };
            Task.WaitAll(exportTasks);

            var originalImgPathPairs = CreateFilePathPairs(exportFolderPath1, exportFolderPath2, "s????.png");
            foreach (var originalImgPathPair in originalImgPathPairs)
            {
                // Create the difference area drawn images with the slide1 and slide2.
                var diffImgPathPair = CreateNewPathPairWithSuffix(originalImgPathPair, ".diff");
                var options = new DiffDrawnImageOptions()
                {
                    BlurSize = new Size(5, 5),
                    FillColor = Color.FromArgb(0, 255, 0),
                };
                DiffDrawnImageMaker.SaveDiffDrawnImage(originalImgPathPair.Path1, diffImgPathPair.Path1, originalImgPathPair.Path2, diffImgPathPair.Path2, options);

                // Create the weighted added images with the original and difference images.
                var addedImgPathPair = CreateNewPathPairWithSuffix(originalImgPathPair, ".added");
                var addedImgTasks = new Task[] {
                    Task.Run(() => { AddedImageMaker.SaveAddedImage(originalImgPathPair.Path1, 0.6, diffImgPathPair.Path1, 0.4, addedImgPathPair.Path1); }),
                    Task.Run(() => { AddedImageMaker.SaveAddedImage(originalImgPathPair.Path2, 0.6, diffImgPathPair.Path2, 0.4, addedImgPathPair.Path2); })
                };
                Task.WaitAll(addedImgTasks);
            }
        }

        private static string CreatePngExportFolder(string ppFilePath)
        {
            var exportFolderPath = Path.Combine(Path.GetDirectoryName(ppFilePath), Path.GetFileNameWithoutExtension(ppFilePath));
            Directory.CreateDirectory(exportFolderPath);
            return exportFolderPath;
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

        private static PathPair CreateNewPathPairWithSuffix(PathPair pathPair, string suffix)
        {
            return new PathPair(
                Path.Combine(Path.GetDirectoryName(pathPair.Path1), Path.GetFileNameWithoutExtension(pathPair.Path1) + suffix + Path.GetExtension(pathPair.Path1)),
                Path.Combine(Path.GetDirectoryName(pathPair.Path2), Path.GetFileNameWithoutExtension(pathPair.Path2) + suffix + Path.GetExtension(pathPair.Path2))
            );
        }
    }
}

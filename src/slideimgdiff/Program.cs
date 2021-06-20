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
            var ppFilePair = new PathPair(args[0], args[1]);

            // Create the output folder structure.
            const string SubfolderNameOriginalImage = "orig";
            const string SubfolderNameDiffImage = "diff";
            const string SubfolderNameAddedImage = "added";
            var subfolderNames = new string[] { SubfolderNameOriginalImage, SubfolderNameDiffImage, SubfolderNameAddedImage };
            var outputFolderRootPathPair = new PathPair(
                CreateOutputFolderStructure(ppFilePair.Path1, subfolderNames),
                CreateOutputFolderStructure(ppFilePair.Path2, subfolderNames)
            );

            // Export the PowerPoint slides as PNG file.
            var exportFolderPathPair = new PathPair(
                Path.Combine(outputFolderRootPathPair.Path1, SubfolderNameOriginalImage),
                Path.Combine(outputFolderRootPathPair.Path2, SubfolderNameOriginalImage)
            );
            var exportTasks = new Task[] {
                Task.Run(() => { PowerPointExporter.ExportAsPng(ppFilePair.Path1, exportFolderPathPair.Path1, "s{0:0000}.png"); }),
                Task.Run(() => { PowerPointExporter.ExportAsPng(ppFilePair.Path2, exportFolderPathPair.Path2, "s{0:0000}.png"); })
            };
            Task.WaitAll(exportTasks);

            var originalImagePathPairs = CreateFilePathPairs(exportFolderPathPair.Path1, exportFolderPathPair.Path2, "s????.png");
            foreach (var originalImagePathPair in originalImagePathPairs)
            {
                // Create the difference area drawn images with the slide1 and slide2.
                var diffImagePathPair = CreateNewPathPairWithDifferentSubfolder(originalImagePathPair, SubfolderNameDiffImage);
                var options = new DiffDrawnImageOptions()
                {
                    BlurSize = new Size(5, 5),
                    FillColor = Color.FromArgb(0, 255, 0),
                };
                DiffDrawnImageMaker.SaveDiffDrawnImage(originalImagePathPair.Path1, diffImagePathPair.Path1, originalImagePathPair.Path2, diffImagePathPair.Path2, options);

                // Create the weighted added images with the original and difference images.
                var addedImagePathPair = CreateNewPathPairWithDifferentSubfolder(originalImagePathPair, SubfolderNameAddedImage);
                var addedImageTasks = new Task[] {
                    Task.Run(() => { AddedImageMaker.SaveAddedImage(originalImagePathPair.Path1, 0.6, diffImagePathPair.Path1, 0.4, addedImagePathPair.Path1); }),
                    Task.Run(() => { AddedImageMaker.SaveAddedImage(originalImagePathPair.Path2, 0.6, diffImagePathPair.Path2, 0.4, addedImagePathPair.Path2); })
                };
                Task.WaitAll(addedImageTasks);
            }
        }

        private static string CreateOutputFolderStructure(string ppFilePath, string[] subfolders)
        {
            var exportFolderPath = Path.Combine(Path.GetDirectoryName(ppFilePath), Path.GetFileNameWithoutExtension(ppFilePath));
            Directory.CreateDirectory(exportFolderPath);
            foreach (var subfolder in subfolders)
            {
                Directory.CreateDirectory(Path.Combine(exportFolderPath, subfolder));
            }
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

        private static PathPair CreateNewPathPairWithDifferentSubfolder(PathPair pathPair, string subfolderName)
        {
            var rootFolderPath1 = Path.GetDirectoryName(Path.GetDirectoryName(pathPair.Path1));
            var rootFolderPath2 = Path.GetDirectoryName(Path.GetDirectoryName(pathPair.Path2));
            return new PathPair(
                Path.Combine(rootFolderPath1, subfolderName, Path.GetFileName(pathPair.Path1)),
                Path.Combine(rootFolderPath2, subfolderName, Path.GetFileName(pathPair.Path2))
            );
        }
    }
}

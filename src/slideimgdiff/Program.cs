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

            pathCtx1.CreateOutputFolders();
            pathCtx2.CreateOutputFolders();

            // Export the PowerPoint slides as PNG image file.
            SaveOriginalImages(pathCtx1, pathCtx2);

            var originalImagePathPairs = CreateFilePathPairs(pathCtx1.OriginalImageOutputFolderPath, pathCtx2.OriginalImageOutputFolderPath, "s????.png");
            foreach (var originalImagePathPair in originalImagePathPairs)
            {
                // Create the difference area drawn images with the original image1 and image2.
                var diffImagePathPair = CreateAndSaveDiffImage(originalImagePathPair, new PathPair(pathCtx1.DiffImageOutputFolderPath, pathCtx2.DiffImageOutputFolderPath));

                // Create the weighted added images with the original and difference images.
                CreateAndSaveAddedImage(originalImagePathPair, diffImagePathPair, new PathPair(pathCtx1.AddedImageOutputFolderPath, pathCtx2.AddedImageOutputFolderPath));
            }
        }

        private static void SaveOriginalImages(PathContext pathCtx1, PathContext pathCtx2)
        {
            const string ExportFileNamePattern = "s{0:0000}.png";
            var exportTasks = new Task[] {
                Task.Run(() => { PowerPointExporter.ExportAsPng(pathCtx1.InputFilePath, pathCtx1.OriginalImageOutputFolderPath, ExportFileNamePattern); }),
                Task.Run(() => { PowerPointExporter.ExportAsPng(pathCtx2.InputFilePath, pathCtx2.OriginalImageOutputFolderPath, ExportFileNamePattern); })
            };
            Task.WaitAll(exportTasks);
        }

        private static PathPair CreateAndSaveDiffImage(PathPair originalImagePathPair, PathPair diffImageOutputFolderPathPair)
        {
            var diffImagePathPair = CreateFolderReplacedNewPathPair(originalImagePathPair, diffImageOutputFolderPathPair);
            var options = new DiffDrawnImageOptions()
            {
                BlurSize = new Size(5, 5),
                FillColor = Color.FromArgb(0, 255, 0),
            };
            DiffDrawnImageMaker.SaveDiffDrawnImage(originalImagePathPair.Path1, diffImagePathPair.Path1, originalImagePathPair.Path2, diffImagePathPair.Path2, options);
            return diffImagePathPair;
        }

        private static PathPair CreateAndSaveAddedImage(PathPair originalImagePathPair, PathPair diffImagePathPair, PathPair addedImageOutputFolderPathPair)
        {
            var addedImagePathPair = CreateFolderReplacedNewPathPair(originalImagePathPair, addedImageOutputFolderPathPair);
            var addedImageTasks = new Task[] {
                    Task.Run(() => { AddedImageMaker.SaveAddedImage(originalImagePathPair.Path1, 0.6, diffImagePathPair.Path1, 0.4, addedImagePathPair.Path1); }),
                    Task.Run(() => { AddedImageMaker.SaveAddedImage(originalImagePathPair.Path2, 0.6, diffImagePathPair.Path2, 0.4, addedImagePathPair.Path2); })
                };
            Task.WaitAll(addedImageTasks);
            return addedImagePathPair;
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

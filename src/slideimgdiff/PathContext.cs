using System.IO;

namespace ppimgdiff
{
    internal class PathContext
    {
        public string InputFilePath { get; protected set; }
        public string RootOutputFolderPath { get; protected set; }
        public string OriginalImageOutputFolderPath { get; protected set; }
        public string DiffImageOutputFolderPath { get; protected set; }
        public string AddedImageOutputFolderPath { get; protected set; }

        public PathContext(string inputPpFilePath)
        {
            InputFilePath = inputPpFilePath;
            RootOutputFolderPath = GetRootOutputFolderPath(inputPpFilePath);
            OriginalImageOutputFolderPath = Path.Combine(RootOutputFolderPath, "orig");
            DiffImageOutputFolderPath = Path.Combine(RootOutputFolderPath, "diff");
            AddedImageOutputFolderPath = Path.Combine(RootOutputFolderPath, "added");
        }

        public void CreateOutputFolders()
        {
            Directory.CreateDirectory(RootOutputFolderPath);
            Directory.CreateDirectory(OriginalImageOutputFolderPath);
            Directory.CreateDirectory(DiffImageOutputFolderPath);
            Directory.CreateDirectory(AddedImageOutputFolderPath);
        }

        private static string GetRootOutputFolderPath(string inputFilePath)
        { 
            return Path.Combine(Path.GetDirectoryName(inputFilePath), Path.GetFileNameWithoutExtension(inputFilePath));
        }
    }
}

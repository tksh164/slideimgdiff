using System.IO;

namespace ppimgdiff
{
    internal class PathContext
    {
        public string InputFilePath { get; protected set; }
        public string RootWorkFolderPath { get; protected set; }
        public string SourceImageFolderPath { get; protected set; }
        public string ResultImageFolderPath { get; protected set; }

        public PathContext(string inputPpFilePath)
        {
            InputFilePath = inputPpFilePath;
            RootWorkFolderPath = GetRootWorkFolderPath(inputPpFilePath);
            SourceImageFolderPath = Path.Combine(RootWorkFolderPath, "source");
            ResultImageFolderPath = Path.Combine(RootWorkFolderPath, "result");
        }

        public void CreateWorkFolders()
        {
            Directory.CreateDirectory(RootWorkFolderPath);
            Directory.CreateDirectory(SourceImageFolderPath);
            Directory.CreateDirectory(ResultImageFolderPath);
        }

        private static string GetRootWorkFolderPath(string inputFilePath)
        { 
            return Path.Combine(Path.GetDirectoryName(inputFilePath), Path.GetFileNameWithoutExtension(inputFilePath));
        }
    }
}

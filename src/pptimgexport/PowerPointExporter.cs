using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Office.Core;
using PP = Microsoft.Office.Interop.PowerPoint;

namespace libppexport
{
    public class PowerPointExporter
    {
        public static void ExportAsPng(string powerPointFileFullPath, string outputFolderFullPath, string fileNamePattern = "slide{0:0000}.png")
        {
            SaveCopyAsPng(powerPointFileFullPath, outputFolderFullPath);
            RenameSavedFiles(outputFolderFullPath, fileNamePattern);
        }

        private static void SaveCopyAsPng(string powerPointFileFullPath, string outputFolderFullPath)
        {
            var ppApp = new PP.Application();
            try
            {
                var slide = ppApp.Presentations.Open(powerPointFileFullPath, MsoTriState.msoTrue, MsoTriState.msoFalse, MsoTriState.msoFalse);
                slide.SaveCopyAs(outputFolderFullPath, PP.PpSaveAsFileType.ppSaveAsPNG, MsoTriState.msoTriStateMixed);
                slide.Close();
            }
            finally
            {
                ppApp.Quit();
            }
        }

        private static void RenameSavedFiles(string outputFolderFullPath, string fileNamePattern)
        {
            var regex = new Regex(@"^slide(?<num>[0-9]+)\.png$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                MatchCasing = MatchCasing.CaseInsensitive,
                MatchType = MatchType.Simple,
                RecurseSubdirectories = false,
                ReturnSpecialDirectories = false,
            };
            foreach (var filePath in Directory.EnumerateFiles(outputFolderFullPath, "slide*.png", options))
            {
                var match = regex.Match(Path.GetFileName(filePath));
                var num = uint.Parse(match.Groups["num"].Value);
                var newFilePath = Path.Combine(Path.GetDirectoryName(filePath), string.Format(fileNamePattern, num));
                File.Move(filePath, newFilePath, true);
            }
        }
    }
}

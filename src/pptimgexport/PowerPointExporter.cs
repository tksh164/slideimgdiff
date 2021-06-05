using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace libppexport
{
    public class PowerPointExporter
    {
        public static void ExportAsPng(string powerPointFileFullPath, string outputFolderFullPath)
        {
            var ppApp = new PowerPoint.Application();
            try
            {
                var slide = ppApp.Presentations.Open(powerPointFileFullPath, MsoTriState.msoTrue, MsoTriState.msoFalse, MsoTriState.msoFalse);
                slide.SaveCopyAs(outputFolderFullPath, PowerPoint.PpSaveAsFileType.ppSaveAsPNG, MsoTriState.msoTriStateMixed);
                slide.Close();
            }
            finally
            {
                ppApp.Quit();
            }

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
                var newFilePath = Path.Combine(Path.GetDirectoryName(filePath), string.Format("s{0:0000}.png", num));
                File.Move(filePath, newFilePath, true);
            }
        }
    }
}

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
            }
            finally
            {
                ppApp.Quit();
            }
        }
    }
}

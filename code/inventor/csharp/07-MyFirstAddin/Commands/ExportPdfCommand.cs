using CadAutomation.Export;
using Inventor;
using IO = System.IO;

namespace MyFirstAddin.Commands
{
    /// <summary>Exports the active drawing to PDF next to the .idw/.dwg and opens the folder.</summary>
    public sealed class ExportPdfCommand : AddinCommand
    {
        public override string DisplayName => "Export PDF";
        public override string InternalName => "CadAutomation_ExportPdf";
        public override string Tooltip => "Save all sheets of the active drawing as a PDF next to the drawing";
        public override string IconText => "P";
        public override bool AppliesTo(string ribbonName) => ribbonName == "Drawing";

        protected override void Execute()
        {
            if (App.ActiveDocument is not DrawingDocument dwg)
                throw new InvalidOperationException("Open a drawing first.");
            if (string.IsNullOrEmpty(dwg.FullFileName))
                throw new InvalidOperationException("Save the drawing first.");
            string pdf = IO.Path.ChangeExtension(dwg.FullFileName, ".pdf");
            new Exporter(App).ToPdf((Document)dwg, pdf);
            App.StatusBarText = "PDF saved: " + pdf;
            System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + pdf + "\"");
        }
    }
}

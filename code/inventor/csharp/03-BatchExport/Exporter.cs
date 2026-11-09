// Exporter.cs - translator wrappers shared by the BatchExport EXE (and copy-pasteable into an add-in).
// Translator GUIDs are fixed Autodesk class IDs (see docs/09 and the API help "Translator samples").
using Inventor;

namespace CadAutomation.Export
{
    public sealed class Exporter
    {
        public const string PdfAddinId  = "{0AC6FD96-2F4D-42CE-8BE0-8AEA580399E4}";
        public const string DxfAddinId  = "{C24E3AC4-122E-11D5-8E91-0010B541CD80}";
        public const string DwgAddinId  = "{C24E3AC2-122E-11D5-8E91-0010B541CD80}";
        public const string StepAddinId = "{90AF7F40-0C01-11D5-8E83-0010B541CD80}";

        private readonly Inventor.Application _app;
        public Exporter(Inventor.Application app) => _app = app;

        private TranslatorAddIn GetTranslator(string id)
        {
            var addin = (TranslatorAddIn)_app.ApplicationAddIns.ItemById[id];
            if (!addin.Activated) addin.Activate();
            return addin;
        }

        private (TranslationContext ctx, NameValueMap opts, DataMedium dm) NewIo(string outFile)
        {
            TranslationContext ctx = _app.TransientObjects.CreateTranslationContext();
            ctx.Type = IOMechanismEnum.kFileBrowseIOMechanism;
            NameValueMap opts = _app.TransientObjects.CreateNameValueMap();
            DataMedium dm = _app.TransientObjects.CreateDataMedium();
            dm.FileName = outFile;
            return (ctx, opts, dm);
        }

        /// <summary>All sheets of a drawing to one PDF.</summary>
        public void ToPdf(Document drawing, string outFile, bool allBlack = false, int dpi = 400)
        {
            TranslatorAddIn pdf = GetTranslator(PdfAddinId);
            var (ctx, opts, dm) = NewIo(outFile);
            if (pdf.HasSaveCopyAsOptions[drawing, ctx, opts])
            {
                opts.Value["All_Color_AS_Black"] = allBlack ? 1 : 0;
                opts.Value["Remove_Line_Weights"] = 0;
                opts.Value["Vector_Resolution"] = dpi;
                opts.Value["Sheet_Range"] = PrintRangeEnum.kPrintAllSheets;
            }
            pdf.SaveCopyAs(drawing, ctx, opts, dm);
        }

        /// <summary>
        /// Drawing to DXF. Pass an .ini created once from File > Save As > Save Copy As > DXF > Options > Save Configuration.
        /// Without an ini Inventor uses defaults (one DXF per sheet, file names get a _SheetN suffix for multi-sheet drawings).
        /// </summary>
        public void DrawingToDxf(Document drawing, string outFile, string? iniFile = null)
        {
            TranslatorAddIn dxf = GetTranslator(DxfAddinId);
            var (ctx, opts, dm) = NewIo(outFile);
            if (iniFile != null && dxf.HasSaveCopyAsOptions[drawing, ctx, opts])
                opts.Value["Export_Acad_IniFile"] = iniFile;
            dxf.SaveCopyAs(drawing, ctx, opts, dm);
        }

        /// <summary>Part/assembly to STEP AP214 (ApplicationProtocolType 3) - AP242 = 5 on recent releases (verify in your version).</summary>
        public void ToStep(Document model, string outFile, int protocol = 3)
        {
            TranslatorAddIn step = GetTranslator(StepAddinId);
            var (ctx, opts, dm) = NewIo(outFile);
            if (step.HasSaveCopyAsOptions[model, ctx, opts])
            {
                opts.Value["ApplicationProtocolType"] = protocol;
                opts.Value["Author"] = System.Environment.UserName;
            }
            step.SaveCopyAs(model, ctx, opts, dm);
        }

        /// <summary>Sheet-metal flat pattern to DXF using DataIO (no translator needed).</summary>
        public bool FlatPatternToDxf(PartDocument part, string outFile)
        {
            if (part.ComponentDefinition is not SheetMetalComponentDefinition smcd) return false;
            if (!smcd.HasFlatPattern)
            {
                smcd.Unfold();      // creates the flat pattern (and makes it the active edit)
                smcd.FlatPattern.ExitEdit();
            }
            // Format string options documented in API help "DataIO.WriteDataToFile" / forum samples:
            string fmt = "FLAT PATTERN DXF?AcadVersion=2018" +
                         "&OuterProfileLayer=OUTER&InteriorProfilesLayer=INNER" +
                         "&InvisibleLayers=IV_TANGENT;IV_BEND;IV_BEND_DOWN;IV_ARC_CENTERS;IV_TOOL_CENTER;IV_TOOL_CENTER_DOWN;IV_FEATURE_PROFILES;IV_FEATURE_PROFILES_DOWN;IV_ALTREP_FRONT;IV_ALTREP_BACK;IV_ROLL_TANGENT;IV_ROLL;IV_UNCONSUMED_SKETCHES";
            smcd.DataIO.WriteDataToFile(fmt, outFile);
            return true;
        }
    }
}

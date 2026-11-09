// 03-BatchExport (C# EXE): walk a folder and export
//   drawings (.idw/.dwg) -> PDF (+ optional DXF)
//   parts/assemblies (.ipt/.iam) -> STEP (optional)
//   sheet-metal parts -> flat pattern DXF (optional)
// Usage:
//   BatchExport <folder> [--out <dir>] [--pdf] [--dxf] [--step] [--flat] [--recurse] [--ini <dxf.ini>] [--black]
//   BatchExport "D:\Vault\Project A" --pdf --flat --recurse --out D:\Export
// Writes export_log.csv next to the outputs. Inventor may be running or not (a hidden one is started).
using CadAutomation.Export;
using CadAutomation.Shared;
using Inventor;
using IO = System.IO;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 0) { Console.WriteLine("usage: BatchExport <folder> [--out dir] [--pdf] [--dxf] [--step] [--flat] [--recurse] [--ini file] [--black]"); return 2; }
        string src = IO.Path.GetFullPath(args[0]);
        string Opt(string name) { int i = Array.IndexOf(args, name); return i >= 0 && i + 1 < args.Length ? args[i + 1] : ""; }
        bool Has(string name) => args.Contains(name, StringComparer.OrdinalIgnoreCase);
        string outDir = Opt("--out"); if (outDir == "") outDir = IO.Path.Combine(src, "_export");
        bool pdf = Has("--pdf"), dxf = Has("--dxf"), step = Has("--step"), flat = Has("--flat");
        if (!(pdf || dxf || step || flat)) pdf = true;
        string? ini = Opt("--ini") is { Length: > 0 } s ? s : null;
        var search = Has("--recurse") ? IO.SearchOption.AllDirectories : IO.SearchOption.TopDirectoryOnly;
        IO.Directory.CreateDirectory(outDir);

        bool weStarted = InventorConnector.GetActiveObject(InventorConnector.ProgId) == null;
        Inventor.Application app = InventorConnector.Connect(startIfNotRunning: true, visible: false);
        var ex = new Exporter(app);
        using var log = new IO.StreamWriter(IO.Path.Combine(outDir, "export_log.csv"));
        log.WriteLine("file,action,output,status,message");

        app.SilentOperation = true;    // suppress "missing reference" / "migrate" dialogs
        int ok = 0, fail = 0;
        try
        {
            var files = IO.Directory.EnumerateFiles(src, "*.*", search)
                .Where(f => f.EndsWith(".idw", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".dwg", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".ipt", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".iam", StringComparison.OrdinalIgnoreCase))
                .Where(f => !f.Contains(@"\OldVersions\", StringComparison.OrdinalIgnoreCase))
                .ToList();
            Console.WriteLine($"{files.Count} files");

            foreach (string f in files)
            {
                string ext = IO.Path.GetExtension(f).ToLowerInvariant();
                string stem = IO.Path.Combine(outDir, IO.Path.GetFileNameWithoutExtension(f));
                // skip .dwg that are plain AutoCAD drawings (not Inventor DWG)
                if (ext == ".dwg" && app.FileManager.IsInventorDWG(f) == false) continue;
                bool needModel = ext is ".ipt" or ".iam";
                if (needModel && !(step || (flat && ext == ".ipt"))) continue;
                if (!needModel && !(pdf || dxf)) continue;

                Document? doc = null;
                bool wasOpen = false;
                try
                {
                    foreach (Document d in app.Documents)
                        if (string.Equals(d.FullFileName, f, StringComparison.OrdinalIgnoreCase)) { doc = d; wasOpen = true; break; }
                    doc ??= app.Documents.Open(f, false);   // false = invisible: much faster, no graphics

                    if (!needModel)
                    {
                        if (pdf) Do(f, "pdf", stem + ".pdf", () => ex.ToPdf(doc, stem + ".pdf", Has("--black")));
                        if (dxf) Do(f, "dxf", stem + ".dxf", () => ex.DrawingToDxf(doc, stem + ".dxf", ini));
                    }
                    else
                    {
                        if (step) Do(f, "step", stem + ".stp", () => ex.ToStep(doc, stem + ".stp"));
                        if (flat && doc is PartDocument p && p.ComponentDefinition is SheetMetalComponentDefinition)
                            Do(f, "flat", stem + "_flat.dxf", () => ex.FlatPatternToDxf(p, stem + "_flat.dxf"));
                    }
                }
                catch (Exception e) { fail++; Log(f, "open", "", "FAIL", e.Message); }
                finally
                {
                    if (doc != null && !wasOpen) doc.Close(true);   // true = skip save
                }
            }
        }
        finally
        {
            app.SilentOperation = false;
            if (weStarted) app.Quit();
        }
        Console.WriteLine($"done: {ok} ok, {fail} failed -> {outDir}");
        return fail == 0 ? 0 : 1;

        void Do(string file, string action, string output, System.Action a)
        {
            try { a(); ok++; Log(file, action, output, "OK", ""); Console.WriteLine($"OK   {action,-4} {IO.Path.GetFileName(file)}"); }
            catch (Exception e) { fail++; Log(file, action, output, "FAIL", e.Message); Console.WriteLine($"FAIL {action,-4} {IO.Path.GetFileName(file)}: {e.Message}"); }
        }
        void Log(string file, string action, string output, string status, string msg) =>
            log.WriteLine(string.Join(",", new[] { file, action, output, status, msg }.Select(Csv)));
        static string Csv(string v) => "\"" + v.Replace("\"", "\"\"") + "\"";
    }
}

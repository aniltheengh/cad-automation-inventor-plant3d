// Report.cs - small helpers that need no Plant 3D type: safe command wrapper, output folder, CSV text.
using System.Text;
using Autodesk.AutoCAD.EditorInput;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Plant3DAddinStarter
{
    internal static class Report
    {
        /// <summary>
        /// Runs the body of a command and turns the usual failures into ONE readable line on the command line.
        /// The body is a separate method on purpose: .NET loads the Plant 3D assemblies when that method is
        /// first called, so "assembly not found" (plain AutoCAD) is thrown inside this try block.
        /// </summary>
        public static void Run(string command, Action body)
        {
            Editor? ed = AcApp.DocumentManager.MdiActiveDocument?.Editor;
            try
            {
                body();
            }
            catch (InvalidOperationException ex)        // our own "nothing to work on" messages
            {
                ed?.WriteMessage($"\n{command}: {ex.Message}\n");
            }
            catch (System.Exception ex) when (ex is FileNotFoundException or FileLoadException or TypeLoadException)
            {
                ed?.WriteMessage($"\n{command}: a Plant 3D assembly could not be loaded ({ex.Message}). " +
                                 "Run this in AutoCAD Plant 3D, not in plain AutoCAD.\n");
            }
            catch (System.Exception ex)                 // System.Exception: AutoCAD has its own Exception class
            {
                ed?.WriteMessage($"\n{command} failed: {ex.GetType().Name}: {ex.Message}\n");
            }
        }

        /// <summary>Documents\Plant3DAddinStarter - the samples never write into the project folder.</summary>
        public static string OutputFolder()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), PlantAddin.Name);
            Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>File name such as P3D-Model-01_components_20261113_0915.csv in the output folder.</summary>
        public static string OutputFile(string drawingPath, string suffix, string extension)
        {
            string name = Path.GetFileNameWithoutExtension(drawingPath);
            if (string.IsNullOrWhiteSpace(name)) name = "drawing";
            return Path.Combine(OutputFolder(), $"{name}_{suffix}_{DateTime.Now:yyyyMMdd_HHmm}.{extension}");
        }

        /// <summary>One CSV field: always quoted, quotes doubled.</summary>
        public static string Csv(string? value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";

        public static string CsvLine(IEnumerable<string?> values) => string.Join(",", values.Select(Csv));

        /// <summary>UTF-8 with BOM: Excel then shows Japanese and other non-ASCII text correctly.</summary>
        public static void WriteText(string path, string text) => File.WriteAllText(path, text, new UTF8Encoding(true));
    }
}

// PlantHelpers.cs - small wrappers around the Plant 3D project + data (PnPDatabase) API.
using System.Text;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.DataObjects;
using Autodesk.ProcessPower.P3dProjectParts;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.ProjectManager;

namespace Plant3DDataTools
{
    internal static class PlantHelpers
    {
        /// <summary>"Piping" (3D), "PnId" (P&ID), "Ortho", "Iso" - names from the project's Project.xml.</summary>
        public static Project Part(string name)
        {
            PlantProject prj = PlantApplication.CurrentProject
                ?? throw new InvalidOperationException("No Plant 3D project is open (Project Manager).");
            return prj.ProjectParts[name] ?? throw new InvalidOperationException($"Project part '{name}' not found.");
        }

        public static PipingProject Piping() => (PipingProject)Part("Piping");
        public static DataLinksManager Dlm(string part) => Part(part).DataLinksManager;

        /// <summary>Every column of every row of a table, as CSV (UTF-8 BOM for Excel).</summary>
        public static int TableToCsv(PnPDatabase db, string tableName, string csvPath, string? filter = null)
        {
            PnPTable t = db.Tables[tableName] ?? throw new ArgumentException("No table " + tableName);
            var cols = t.Columns.Select(c => c.Name).ToList();
            PnPRow[] rows = filter == null ? t.Select() : t.Select(filter);
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", cols.Select(Csv)));
            foreach (PnPRow r in rows)
                sb.AppendLine(string.Join(",", cols.Select(c => Csv(Convert.ToString(r[c]) ?? ""))));
            File.WriteAllText(csvPath, sb.ToString(), new UTF8Encoding(true));
            return rows.Length;
        }

        public static string Csv(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";

        // PlantProject.ProjectFolderPath: the member Autodesk's own sample uses (CLIPLANT360SHARE, 2024).
        // ProjectDirectory belongs to a project PART (Project), not to PlantProject - see docs/11a.
        public static string ProjectFolder() => PlantApplication.CurrentProject.ProjectFolderPath;
    }
}

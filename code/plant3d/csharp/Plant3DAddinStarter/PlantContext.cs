// PlantContext.cs - "connect" once, then every command works with the same five objects:
//
//   AutoCAD     Application -> Document (the active drawing) -> Database + Editor
//   Plant 3D    PlantApplication.CurrentProject -> project part of this drawing -> DataLinksManager -> PnPDatabase
//
// An add-in runs INSIDE Plant 3D, so there is nothing to start or to log in to: the static classes
// Application and PlantApplication are simply there. Explained in docs/11c-plant3d-code-examples.md.
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.DataObjects;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.ProjectManager;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Plant3DAddinStarter
{
    internal sealed class PlantContext
    {
        public Document Doc { get; }
        public Database Db => Doc.Database;
        public Editor Ed => Doc.Editor;
        public PlantProject Project { get; }
        /// <summary>Name of the project part the active drawing belongs to: "Piping", "PnId", ...</summary>
        public string PartName { get; }
        public Project Part { get; }
        /// <summary>The link between DWG objects (ObjectId) and project database rows (row id).</summary>
        public DataLinksManager Dlm { get; }
        /// <summary>The project database of this part: tables = classes, rows = objects.</summary>
        public PnPDatabase Data { get; }

        private PlantContext(Document doc, PlantProject project, string partName, Project part)
        {
            Doc = doc;
            Project = project;
            PartName = partName;
            Part = part;
            Dlm = part.DataLinksManager;
            Data = Dlm.GetPnPDatabase();
        }

        /// <summary>Application -> active document -> current project -> project part of that document.</summary>
        public static PlantContext FromActiveDrawing()
        {
            // 1) the active document (null when no drawing is open)
            Document doc = AcApp.DocumentManager.MdiActiveDocument
                ?? throw new InvalidOperationException("No drawing is open.");

            // 2) the Plant project that is current in Project Manager (null when none is open)
            PlantProject project = PlantApplication.CurrentProject
                ?? throw new InvalidOperationException("No Plant 3D project is open (Project Manager).");

            // 3) which part of the project does this drawing belong to?  "PnID", "Piping", ...
            string docType = PnPProjectUtils.GetActiveDocumentType() ?? "";
            Project? part = FindPart(project, docType);
            if (part == null)
                throw new InvalidOperationException(
                    $"The active drawing is not a P&ID or 3D model drawing of project '{project.Name}' " +
                    $"(drawing type '{docType}', project parts: {string.Join(", ", PartNames(project))}).");

            return new PlantContext(doc, project, project.ProjectPartName(part), part);
        }

        /// <summary>Find a project part by name, ignoring case ("PnId" and "PnID" both appear in samples).</summary>
        public static Project? FindPart(PlantProject project, string name)
        {
            foreach (Project part in project.ProjectParts)
                if (string.Equals(project.ProjectPartName(part), name, StringComparison.OrdinalIgnoreCase))
                    return part;
            return null;
        }

        public static List<string> PartNames(PlantProject project)
        {
            var names = new List<string>();
            foreach (Project part in project.ProjectParts) names.Add(project.ProjectPartName(part));
            return names;
        }

        // ---------------------------------------------------------------- reading data of one object

        /// <summary>
        /// Row id (PnPID) of a DWG object, or false for plain AutoCAD geometry (lines, text, blocks, xrefs).
        /// HasLinks is the cheap test; the try/catch covers releases where FindAcPpRowId throws instead.
        /// </summary>
        public bool TryGetRowId(ObjectId id, out int rowId)
        {
            rowId = 0;
            try
            {
                if (!Dlm.HasLinks(id)) return false;
                rowId = Dlm.FindAcPpRowId(id);
            }
            catch (System.Exception)
            {
                return false;
            }
            return rowId > 0;
        }

        /// <summary>Every property of a row as name -> value (all values arrive as text).</summary>
        public Dictionary<string, string> Properties(int rowId)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> p in Dlm.GetAllProperties(rowId, true))
                if (p.Key != null) result[p.Key] = p.Value ?? "";
            return result;
        }

        /// <summary>
        /// The project class of an object and its parents, e.g. "PipeRunComponent > Elbow". A project class is a
        /// table in the project database; BaseTableName leads from a table to its parent until "PnPBase".
        /// </summary>
        public string ClassPath(string className)
        {
            var levels = new List<string>();
            string? current = className;
            while (!string.IsNullOrWhiteSpace(current) &&
                   !string.Equals(current, "PnPBase", StringComparison.OrdinalIgnoreCase) &&
                   levels.Count < 20)                               // guard against a circular definition
            {
                levels.Add(current);
                current = Data.Tables.Contains(current) ? Data.Tables[current].BaseTableName : null;
            }
            levels.Reverse();
            return string.Join(" > ", levels);
        }

        /// <summary>
        /// 3D model: the row id of the line group (line number) a part belongs to, found through the relationship
        /// P3dLineGroupPartRelationship (roles "Part" and "LineGroup"). Returns 0 when there is none.
        /// </summary>
        public int LineGroupRowId(int partRowId)
        {
            try
            {
                foreach (int id in Dlm.GetRelatedRowIds("P3dLineGroupPartRelationship", "Part", partRowId, "LineGroup"))
                    return id;                                      // a part belongs to one line group
            }
            catch (System.Exception)
            {
                // P&ID drawing, or a class that takes no part in this relationship
            }
            return 0;
        }

        /// <summary>Text such as "row 5123, Tag '2"-CW-1001', Number '1001'"; "" when the part has no line group.</summary>
        public string LineGroupLabel(int partRowId)
        {
            int id = LineGroupRowId(partRowId);
            if (id <= 0) return "";
            PnPRow? row = Data.GetRow(id);
            return row == null ? $"row {id}" : $"row {id}, Tag '{Value(row, "Tag")}', Number '{Value(row, "Number")}'";
        }

        /// <summary>One value of a row, "" when the table has no such column (column names differ per project).</summary>
        public static string Value(PnPRow row, string column) =>
            row.Table.Columns.Contains(column) ? Convert.ToString(row[column]) ?? "" : "";
    }
}

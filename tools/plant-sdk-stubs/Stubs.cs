// Stubs mirroring the public Plant 3D .NET API surface used in code/plant3d. Source for each member:
//  LAB_01..05 in https://github.com/ADN-DevTech/Plant3DTraining  and the forum/blog posts cited in docs/11.
using System.Collections.Generic;
using System.Collections.Specialized;
using Autodesk.AutoCAD.DatabaseServices;

namespace Autodesk.ProcessPower.PlantInstance
{
    public static class PlantApplication { public static Autodesk.ProcessPower.ProjectManager.PlantProject CurrentProject => null; }
}

namespace Autodesk.ProcessPower.ProjectManager
{
    public class Project
    {
        public string ProjectName => null;
        public string ProjectDirectory => null;
        public Autodesk.ProcessPower.DataLinks.DataLinksManager DataLinksManager => null;
        public List<PnPProjectDrawing> GetPnPDrawingFiles() => null;
    }
    public class PlantProject : Project
    {
        public ProjectPartCollection ProjectParts => null;
        public string ProjectPartName(Project p) => null;
    }
    public class ProjectPartCollection : IEnumerable<Project>
    {
        public Project this[string name] => null;
        public IEnumerator<Project> GetEnumerator() => null;
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
    }
    public class PnPProjectDrawing { public string AbsoluteFileName => null; }
}

namespace Autodesk.ProcessPower.P3dProjectParts
{
    public class PipingProject : Autodesk.ProcessPower.ProjectManager.Project
    {
        public double MinimumPipeLength { get; set; }
        public double WeldGapSize { get; set; }
        public bool UseWeldGaps { get; set; }
    }
}

namespace Autodesk.ProcessPower.DataObjects
{
    public class PnPDatabase
    {
        public PnPTables Tables => null;
        public PnPRow GetRow(int rowId) => null;
        public string GetRowTableName(int rowId) => null;
        public void StartTransaction() { }
        public void CommitTransaction() { }
    }
    public class PnPTables : IEnumerable<PnPTable>
    {
        public PnPTable this[string name] => null;
        public IEnumerator<PnPTable> GetEnumerator() => null;
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
    }
    public class PnPTable
    {
        public string Name => null;
        public PnPColumns Columns => null;
        public PnPRows Rows => null;
        public PnPRow[] Select() => null;
        public PnPRow[] Select(string filter) => null;
        public PnPRow NewRow() => null;
    }
    public class PnPColumns : IEnumerable<PnPColumn>
    {
        public IEnumerator<PnPColumn> GetEnumerator() => null;
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
    }
    public class PnPColumn { public string Name => null; }
    public class PnPRows { public void Add(PnPRow r) { } }
    public class PnPRow
    {
        public int RowId => 0;
        public object this[string column] { get => null; set { } }
        public void BeginEdit() { }
        public void EndEdit() { }
    }
    public class PnPRowIdArray : LinkedList<int> { }
}

namespace Autodesk.ProcessPower.DataLinks
{
    using Autodesk.ProcessPower.DataObjects;
    public class PpObjectId { }
    public class PpObjectIdArray : List<PpObjectId> { }
    public class DataLinksManager
    {
        public PnPDatabase GetPnPDatabase() => null;
        public int FindAcPpRowId(ObjectId id) => 0;
        public int FindAcPpRowId(PpObjectId id) => 0;
        public PpObjectId MakeAcPpObjectId(ObjectId id) => null;
        public PpObjectIdArray FindAcPpObjectIds(int rowId) => null;
        public ObjectId MakeAcDbObjectId(PpObjectId id) => ObjectId.Null;
        public List<KeyValuePair<string, string>> GetAllProperties(int rowId, bool includeAll) => null;
        public void SetProperties(int rowId, StringCollection names, StringCollection values) { }
        public PnPRowIdArray GetRelatedRowIds(string relationship, string role, int rowId, string otherRole) => null;
        public int GetDrawingId(Database db) => 0;
        public void Relate(string relationship, string role1, int id1, string role2, int id2) { }
        public void Unrelate(string relationship, string role1, int id1, string role2, int id2) { }
    }
}

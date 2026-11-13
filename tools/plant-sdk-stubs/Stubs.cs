// Stubs mirroring the public Plant 3D .NET API surface used in code/plant3d. Source for each member:
//  [LAB]   LAB_01..06 in https://github.com/ADN-DevTech/Plant3DTraining (+ the class diagrams in its presentation)
//  [BLOG]  Autodesk Developer Blog samples (blog.autodesk.io) and github.com/MadhukarMoogala/CLIPLANT360SHARE
//  [SPKL]  github.com/specklesystems/speckle-sharp-connectors (Converters/Plant3d) - open-source code that
//          builds against the real Plant 3D 2026 and 2027 assemblies
//  [FORUM] Autodesk Community posts cited in docs/11 and docs/11a
// Members without a tag were already here and come from the labs / the posts cited in docs/11.
using System.Collections.Generic;
using System.Collections.Specialized;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Autodesk.ProcessPower.PlantInstance
{
    public static class PlantApplication { public static Autodesk.ProcessPower.ProjectManager.PlantProject CurrentProject => null; }
}

namespace Autodesk.ProcessPower.ProjectManager
{
    /// <summary>One project part ("Piping", "PnId", "Ortho", "Iso"...). Abstract in the real SDK.</summary>
    public class Project
    {
        public string ProjectName => null;
        public string ProjectDirectory => null;
        public Autodesk.ProcessPower.DataLinks.DataLinksManager DataLinksManager => null;
        public List<PnPProjectDrawing> GetPnPDrawingFiles() => null;
    }
    /// <summary>The whole Plant project. NOT derived from Project: the ADN class diagram shows it as a separate
    /// class that owns the parts through ProjectParts [LAB].</summary>
    public class PlantProject
    {
        public string Name => null;                          // [BLOG]
        public string ProjectFolderPath => null;             // [BLOG]
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
    public static class PnPProjectUtils
    {
        public static string GetActiveDocumentType() => null;   // [SPKL] "PnID", "Piping", ...
    }
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
        public bool Contains(string name) => false;              // [SPKL]
        public IEnumerator<PnPTable> GetEnumerator() => null;
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
    }
    public class PnPTable
    {
        public string Name => null;
        public string BaseTableName => null;                     // [SPKL] parent class; the root is "PnPBase"
        public PnPColumns Columns => null;
        public PnPRows Rows => null;
        public PnPRow[] Select() => null;
        public PnPRow[] Select(string filter) => null;
        public PnPRow NewRow() => null;
    }
    public class PnPColumns : IEnumerable<PnPColumn>
    {
        public bool Contains(string name) => false;              // [SPKL]
        public IEnumerator<PnPColumn> GetEnumerator() => null;
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
    }
    public class PnPColumn { public string Name => null; }
    public class PnPRows { public void Add(PnPRow r) { } }
    public class PnPRow
    {
        public int RowId => 0;
        public PnPTable Table => null;                           // [LAB] LAB_06, [SPKL]
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
        public static DataLinksManager GetManager(Database db) => null;   // [SPKL] [FORUM]
        public PnPDatabase GetPnPDatabase() => null;
        public bool HasLinks(ObjectId id) => false;                       // [SPKL] cheap "is this a Plant object?" test
        public string GetObjectClassname(ObjectId id) => null;            // [SPKL] project class = table name
        public int FindAcPpRowId(ObjectId id) => 0;
        public int FindAcPpRowId(PpObjectId id) => 0;
        public PpObjectId MakeAcPpObjectId(ObjectId id) => null;
        public PpObjectIdArray FindAcPpObjectIds(int rowId) => null;
        public ObjectId MakeAcDbObjectId(PpObjectId id) => ObjectId.Null;
        public List<KeyValuePair<string, string>> GetAllProperties(int rowId, bool currentVersion) => null;
        public void SetProperties(int rowId, StringCollection names, StringCollection values) { }
        public PnPRowIdArray GetRelatedRowIds(string relationship, string role, int rowId, string otherRole) => null;
        public int GetDrawingId(Database db) => 0;
        public void Relate(string relationship, string role1, int id1, string role2, int id2) { }
        public void Unrelate(string relationship, string role1, int id1, string role2, int id2) { }
    }
}

namespace Autodesk.ProcessPower.PnP3dObjects
{
    // 3D model entities [LAB] LAB_04 + class diagram, [BLOG], [SPKL], [FORUM].
    // Real hierarchy (ADN class diagram): Entity <- Part <- Equipment | Connector | Segment <- Pipe | InlineAsset <- PipeInlineAsset.
    // The stubs skip the two abstract middle classes (Segment, InlineAsset) because no sample code names them.
    public enum PortType { All, Static, Symbolic }
    public class Port
    {
        public string Name => null;
        public Point3d Position => default;
        public Vector3d Direction => default;                    // [BLOG]
    }
    public class PortCollection : System.Collections.IEnumerable   // labs only use foreach + Count
    {
        public int Count => 0;
        public System.Collections.IEnumerator GetEnumerator() => null;
    }
    // All five are "abstract" HERE so that this file compiles whatever abstract members Entity has; the code
    // in code/plant3d only tests types and calls GetPorts. In the real SDK only Part is abstract.
    public abstract class Part : Entity
    {
        // managed wrapper constructor, same pattern as every AutoCAD entity class
        protected Part(System.IntPtr unmanagedPointer, bool autoDelete) : base(unmanagedPointer, autoDelete) { }
        public PortCollection GetPorts(PortType type) => null;
    }
    public abstract class Pipe : Part { protected Pipe(System.IntPtr p, bool a) : base(p, a) { } }
    public abstract class PipeInlineAsset : Part { protected PipeInlineAsset(System.IntPtr p, bool a) : base(p, a) { } }
    public abstract class Connector : Part { protected Connector(System.IntPtr p, bool a) : base(p, a) { } }
    public abstract class Equipment : Part { protected Equipment(System.IntPtr p, bool a) : base(p, a) { } }

    /// <summary>One end of a connection: an object plus one of its ports.</summary>
    public class Pair
    {
        public ObjectId ObjectId { get; set; }
        public Port Port { get; set; }
    }
    public class ConnectionManager
    {
        public bool IsConnected(Pair pair) => false;
        public Pair GetConnectedPairAt(Pair pair) => null;       // [BLOG]
    }
    public class ConnectionIterator
    {
        public static ConnectionIterator NewIterator(ObjectId startId) => null;
        public ObjectId ObjectId => ObjectId.Null;
        public bool Done() => true;
        public void Next() { }
    }
}

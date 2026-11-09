// 01-HelloInventor: the smallest useful Inventor EXE.
// Connects to Inventor, prints version + open documents, then lists the parameters
// and a few iProperties of the active document.
// Run:  dotnet run            (Inventor already open with a part/assembly)
using CadAutomation.Shared;
using Inventor;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        Inventor.Application inv;
        try
        {
            inv = InventorConnector.Connect(startIfNotRunning: false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }

        Console.WriteLine($"Connected to {inv.SoftwareVersion.DisplayName} (build {inv.SoftwareVersion.BuildIdentifier})");
        Console.WriteLine($"Open documents: {inv.Documents.Count}");
        foreach (Document d in inv.Documents)
            Console.WriteLine($"  [{d.DocumentType}] {d.DisplayName}  dirty={d.Dirty}");

        Document? doc = inv.ActiveDocument;
        if (doc == null) { Console.WriteLine("No active document."); return 0; }

        Console.WriteLine($"\nActive: {doc.FullFileName}");

        // iProperties live in PropertySets. Internal names never change with language;
        // display names ("Design Tracking Properties", "Project") do. Use the well-known names below.
        PropertySet summary = doc.PropertySets["Inventor Summary Information"];
        PropertySet design  = doc.PropertySets["Design Tracking Properties"];
        Console.WriteLine($"  Title       : {summary["Title"].Value}");
        Console.WriteLine($"  Part Number : {design["Part Number"].Value}");
        Console.WriteLine($"  Description : {design["Description"].Value}");

        // Parameters exist on Part and Assembly component definitions.
        Parameters? ps = doc switch
        {
            PartDocument p     => p.ComponentDefinition.Parameters,
            AssemblyDocument a => a.ComponentDefinition.Parameters,
            _ => null
        };
        if (ps != null)
        {
            Console.WriteLine($"\nParameters ({ps.Count}):");
            UnitsOfMeasure uom = doc.UnitsOfMeasure;
            foreach (Parameter prm in ps)
            {
                // prm.Value is ALWAYS in database units (cm, rad). Convert for display:
                string shown = prm.Value is double v
                    ? uom.GetStringFromValue(v, prm.get_Units())
                    : prm.Value?.ToString() ?? "";
                Console.WriteLine($"  {prm.Name,-20} = {prm.Expression,-25} -> {shown}");
            }
        }
        return 0;
    }
}

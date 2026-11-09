// 08-ApprenticeReader - read iProperties and the reference tree of Inventor files WITHOUT Inventor running.
//
//   ApprenticeReader <file.iam|.ipt|.idw> [--tree] [--set "Prop=Value"]...
//
// Apprentice = free, in-process subset of the Inventor API. Read: iProperties, file references,
// BOM-like structure, geometry (read-only), thumbnails. Write: iProperties (FlushToFile) and
// reference repair (FileDescriptor.ReplaceReference) + FileSaveAs (copy design). No modelling.
// Fast: no UI, no licence. Typical use: reports, file audits, PDM/ERP export, batch iProperty edits.
using Inventor;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 0) { Console.WriteLine("ApprenticeReader <file> [--tree] [--set \"Name=Value\"]..."); return 2; }
        var apprentice = new ApprenticeServerComponent();
        try
        {
            ApprenticeServerDocument doc = apprentice.Open(args[0]);
            Console.WriteLine($"{doc.DisplayName}  type={doc.DocumentType}");
            foreach (PropertySet set in doc.PropertySets)
            {
                Console.WriteLine($"[{set.DisplayName}]  ({set.Name})");
                foreach (Property p in set)
                {
                    object? v;
                    try { v = p.Value; } catch { v = "<n/a>"; }
                    if (v is not null && v.ToString() != "") Console.WriteLine($"   {p.Name,-28} = {v}");
                }
            }

            var sets = new List<(string name, string val)>();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "--set" && args[i + 1].Split('=', 2) is [var n, var v]) sets.Add((n, v));
            if (sets.Count > 0)
            {
                PropertySet design = doc.PropertySets["Design Tracking Properties"];
                PropertySet custom = doc.PropertySets["Inventor User Defined Properties"];
                foreach (var (n, v) in sets)
                {
                    Property? target = null;
                    foreach (Property p in design) if (p.Name == n) target = p;
                    if (target != null) target.Value = v;
                    else { try { custom[n].Value = v; } catch { custom.Add(v, n); } }
                    Console.WriteLine($"set {n} = {v}");
                }
                doc.PropertySets.FlushToFile();     // writes iProperties only, file stays the same version
            }

            if (args.Contains("--tree")) PrintTree(doc.File, 0, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            doc.Close();
            return 0;
        }
        finally
        {
            apprentice.Close();
        }
    }

    private static void PrintTree(Inventor.File f, int depth, HashSet<string> seen)
    {
        Console.WriteLine(new string(' ', depth * 2) + System.IO.Path.GetFileName(f.FullFileName));
        if (!seen.Add(f.FullFileName)) return;
        foreach (FileDescriptor fd in f.ReferencedFileDescriptors)
        {
            if (fd.ReferenceMissing) { Console.WriteLine(new string(' ', depth * 2 + 2) + "MISSING: " + fd.FullFileName); continue; }
            PrintTree(fd.ReferencedFile, depth + 1, seen);
        }
    }
}

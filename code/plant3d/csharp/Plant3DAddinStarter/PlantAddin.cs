// PlantAddin.cs - the entry point of the add-in.
//   Plant 3D (= AutoCAD) creates ONE object of this class when the DLL is loaded and calls Initialize().
//   Terminate() runs when the program closes - a .NET DLL cannot be unloaded while AutoCAD is running.
//   Guide: docs/11b-plant3d-first-addin.md
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

// Tells AutoCAD which class is the entry point, so it does not have to search every type in the DLL.
// Only ONE class per DLL may carry this attribute. The command classes are announced with
// [assembly: CommandClass(...)] at the top of each *Commands.cs file.
[assembly: ExtensionApplication(typeof(Plant3DAddinStarter.PlantAddin))]

namespace Plant3DAddinStarter
{
    public class PlantAddin : IExtensionApplication
    {
        public const string Name = "Plant3DAddinStarter";

        /// <summary>Called once, when the DLL is loaded (NETLOAD, bundle or registry).</summary>
        public void Initialize()
        {
            // Rule 1: never let an exception escape from Initialize - the commands would not get registered
            //         and all you would see is "Unknown command".
            // Rule 2: do no drawing/project work here. With a bundle or the registry this runs during start-up,
            //         before a drawing or a Plant project is open. Subscribe to events and return quickly.
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += ResolvePlantAssembly;
                AcApp.Idle += OnFirstIdle;          // AutoCAD is ready when it becomes idle for the first time
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"{Name}: Initialize failed: {ex}");
            }
        }

        /// <summary>Called once, when AutoCAD / Plant 3D exits.</summary>
        public void Terminate()
        {
            AcApp.Idle -= OnFirstIdle;
            AppDomain.CurrentDomain.AssemblyResolve -= ResolvePlantAssembly;
        }

        private static void OnFirstIdle(object? sender, EventArgs e)
        {
            AcApp.Idle -= OnFirstIdle;              // run once
            var doc = AcApp.DocumentManager.MdiActiveDocument;      // null while no drawing is open
            doc?.Editor.WriteMessage(
                $"\n{Name} loaded. Commands: P3DHELLO, P3DCOMPONENTS, P3DINSPECT, P3DWALK, P3DAPIMAP, P3DREGISTER, P3DUNREGISTER\n");
        }

        /// <summary>
        /// Safety net: when .NET cannot find a Plant 3D assembly (PnP...dll, PnID...dll) because Plant 3D has
        /// not loaded it yet, look in the folder of acad.exe and in its PLNT3D subfolder.
        /// It only fills gaps INSIDE Plant 3D. In plain AutoCAD no Plant assembly is loaded, and then this
        /// method does nothing: Plant code must not run without the Plant 3D environment behind it.
        /// </summary>
        private static Assembly? ResolvePlantAssembly(object? sender, ResolveEventArgs args)
        {
            try
            {
                string? name = new AssemblyName(args.Name).Name;
                if (!ApiMapCommands.IsPlantName(name)) return null;                     // not ours to resolve
                if (ApiMapCommands.PlantAssemblies().Count == 0) return null;           // this is not Plant 3D

                string acadDir = Path.GetDirectoryName(Environment.ProcessPath) ?? "";  // folder of acad.exe
                foreach (string dir in new[] { Path.Combine(acadDir, "PLNT3D"), acadDir })
                {
                    string path = Path.Combine(dir, name + ".dll");
                    if (File.Exists(path)) return Assembly.LoadFrom(path);
                }
            }
            catch (System.Exception ex)
            {
                // A resolve handler must never throw: .NET calls it in the middle of loading other code.
                System.Diagnostics.Trace.WriteLine($"{Name}: could not resolve {args.Name}: {ex.Message}");
            }
            return null;
        }
    }
}

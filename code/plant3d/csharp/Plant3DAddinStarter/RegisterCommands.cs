// RegisterCommands.cs - "registering" an AutoCAD .NET DLL = telling AutoCAD to load it by itself.
//   P3DREGISTER   : writes the demand-loading values for this DLL under the registry key of the RUNNING
//                   product (for the current Windows user), e.g. for Plant 3D 2026 in English:
//                   HKCU\Software\Autodesk\AutoCAD\R25.1\ACAD-9117:409\Applications\Plant3DAddinStarter
//   P3DUNREGISTER : removes that key again
// No regsvr32, no regasm, no admin rights: four values in HKEY_CURRENT_USER.
// Use EITHER this OR the Autoloader bundle (bundle/PackageContents.xml) - see docs/11b-plant3d-first-addin.md.
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
// Microsoft.Win32 is spelled out below on purpose: Autodesk.AutoCAD.Runtime has its own Registry and
// RegistryKey classes, so a plain "Registry" would be ambiguous.
using Win32 = Microsoft.Win32;

[assembly: CommandClass(typeof(Plant3DAddinStarter.RegisterCommands))]

namespace Plant3DAddinStarter
{
    public class RegisterCommands
    {
        // LOADCTRLS is a sum of flags (enum ApplicationLoadReasons in the AutoCAD .NET reference):
        //    1 = when a drawing with custom objects of this application is opened (proxy detection)
        //    2 = when AutoCAD starts
        //    4 = when an unknown command is typed that this key lists under a "Commands" subkey
        //    8 = on request (of the user or of another application)
        //   16 = do not load          32 = load transparently
        // 14 = 2 + 4 + 8 is the value in Autodesk's own sample (.NET Developer's Guide). Because of the 2 the
        // DLL is in memory from the start, so no "Commands" subkey is written here.
        private const int LoadControls = 14;

        /// <summary>...\R25.1\ACAD-9117:409\Applications of the product that is running right now.</summary>
        private static string ApplicationsKey() =>
            HostApplicationServices.Current.UserRegistryProductRootKey + @"\Applications";

        [CommandMethod("P3DREGISTER")]
        public void Register()
        {
            Document? doc = AcApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;
            Report.Run("P3DREGISTER", () =>
            {
                string dll = Assembly.GetExecutingAssembly().Location;      // where this DLL is right now
                string key = ApplicationsKey() + "\\" + PlantAddin.Name;

                using (Win32.RegistryKey app = Win32.Registry.CurrentUser.CreateSubKey(key))
                {
                    app.SetValue("DESCRIPTION", "Plant 3D add-in starter (learning sample)", Win32.RegistryValueKind.String);
                    app.SetValue("LOADCTRLS", LoadControls, Win32.RegistryValueKind.DWord);
                    app.SetValue("LOADER", dll, Win32.RegistryValueKind.String);
                    app.SetValue("MANAGED", 1, Win32.RegistryValueKind.DWord);      // 1 = .NET DLL
                }
                ed.WriteMessage($"\nRegistered: HKCU\\{key}");
                ed.WriteMessage($"\n  LOADER = {dll}");
                ed.WriteMessage("\nThis product now loads the DLL at every start. Moved or rebuilt elsewhere? Run P3DREGISTER again.");
            });
            ed.WriteMessage("\n");
        }

        [CommandMethod("P3DUNREGISTER")]
        public void Unregister()
        {
            Document? doc = AcApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;
            Report.Run("P3DUNREGISTER", () =>
            {
                using (Win32.RegistryKey? apps = Win32.Registry.CurrentUser.OpenSubKey(ApplicationsKey(), writable: true))
                {
                    if (apps == null || !apps.GetSubKeyNames().Contains(PlantAddin.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        ed.WriteMessage($"\nNot registered for this product (HKCU\\{ApplicationsKey()}).");
                        return;
                    }
                    apps.DeleteSubKeyTree(PlantAddin.Name);
                }
                ed.WriteMessage($"\nRemoved HKCU\\{ApplicationsKey()}\\{PlantAddin.Name}. The DLL stays loaded until you close the program.");
            });
            ed.WriteMessage("\n");
        }
    }
}

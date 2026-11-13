using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(Plant3dSdkSamples.Commands_Project))]
[assembly: CommandClass(typeof(Plant3dSdkSamples.Commands_Properties))]
[assembly: CommandClass(typeof(Plant3dSdkSamples.Commands_Piping))]
[assembly: CommandClass(typeof(Plant3dSdkSamples.Commands_Pid))]
[assembly: CommandClass(typeof(Plant3dSdkSamples.Commands_ExportCsv))]
[assembly: CommandClass(typeof(Plant3dSdkSamples.Commands_Specs))]
[assembly: ExtensionApplication(typeof(Plant3dSdkSamples.PluginApp))]

namespace Plant3dSdkSamples
{
    /// <summary>
    /// Optional startup hook. The assembly-resolve handler is a common fix when
    /// Plant DLLs live under the AutoCAD or PLNT3D folder and .NET does not probe it.
    /// </summary>
    public class PluginApp : IExtensionApplication
    {
        public void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolvePlantOrAcad;
        }

        public void Terminate()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= ResolvePlantOrAcad;
        }

        private static Assembly? ResolvePlantOrAcad(object? sender, ResolveEventArgs e)
        {
            string fileName = new AssemblyName(e.Name).Name + ".dll";
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string[] folders =
            {
                Path.Combine(programFiles, "Autodesk", "AutoCAD 2026", "PLNT3D"),
                Path.Combine(programFiles, "Autodesk", "AutoCAD 2026"),
                Path.Combine(programFiles, "Autodesk", "AutoCAD 2025", "PLNT3D"),
                Path.Combine(programFiles, "Autodesk", "AutoCAD 2025")
            };

            foreach (string folder in folders)
            {
                string path = Path.Combine(folder, fileName);
                if (File.Exists(path))
                    return Assembly.LoadFrom(path);
            }

            return null;
        }
    }
}

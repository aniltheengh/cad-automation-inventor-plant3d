// InventorConnector.cs  —  connect to a running Inventor from .NET 8 (or start one).
//
// Why this file exists:
//   Marshal.GetActiveObject() does not exist in .NET Core / .NET 5+ (so not in .NET 8),
//   which is what Inventor 2025+ uses. We call the same Win32 APIs it wrapped
//   (CLSIDFromProgIDEx + GetActiveObject) through P/Invoke.
// Source of the pattern: Autodesk Developer Blog,
//   "Marshal.GetActiveObject Replacement in .NET 8+: Connecting to Running Autodesk Inventor Instances"
//   https://blog.autodesk.io/marshalgetactiveobject-replacement-in-net-8-connecting-to-running-autodesk-inventor-instances/
//
// Link this file into any EXE project:
//   <Compile Include="..\Shared\InventorConnector.cs" Link="InventorConnector.cs" />

using System;
using System.Runtime.InteropServices;

namespace CadAutomation.Shared
{
    public static class InventorConnector
    {
        public const string ProgId = "Inventor.Application";

        [DllImport("ole32.dll")]
        private static extern int CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

        [DllImport("oleaut32.dll")]
        private static extern int GetActiveObject([MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
                                                  IntPtr reserved,
                                                  [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);

        /// <summary>Returns the running COM object for a ProgID, or null if none is running.</summary>
        public static object? GetActiveObject(string progId)
        {
            int hr = CLSIDFromProgIDEx(progId, out Guid clsid);
            if (hr < 0) return null;                       // ProgID not registered (product not installed)
            hr = GetActiveObject(clsid, IntPtr.Zero, out object obj);
            return hr < 0 ? null : obj;                    // MK_E_UNAVAILABLE (0x800401E3) = not running
        }

        /// <summary>
        /// Get the running Inventor, or optionally start a new hidden/visible one.
        /// Inventor must be the same "elevation" as this EXE (both normal user, or both "Run as admin"),
        /// otherwise the Running Object Table lookup fails.
        /// </summary>
        public static Inventor.Application Connect(bool startIfNotRunning = true, bool visible = true)
        {
            if (GetActiveObject(ProgId) is Inventor.Application running)
                return running;

            if (!startIfNotRunning)
                throw new InvalidOperationException("Inventor is not running. Start Inventor first (same user, not 'Run as administrator').");

            Type t = Type.GetTypeFromProgID(ProgId)
                     ?? throw new InvalidOperationException("Inventor.Application is not registered. Is Inventor installed?");
            var app = (Inventor.Application)Activator.CreateInstance(t)!;
            app.Visible = visible;
            return app;
        }
    }
}

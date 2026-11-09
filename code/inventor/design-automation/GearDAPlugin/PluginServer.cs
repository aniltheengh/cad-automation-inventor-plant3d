using System.Runtime.InteropServices;
using Inventor;

namespace GearDAPlugin
{
    /// <summary>Entry point loaded by InventorCoreConsole.exe on the Design Automation servers.</summary>
    [Guid("BFB706D2-9685-44D5-9D58-50CB749B40DC")]
    [ComVisible(true)]
    public class PluginServer : ApplicationAddInServer
    {
        private InventorServer? _server;
        private GearAutomation? _automation;

        public void Activate(ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            // On DA there is no Inventor.Application - only InventorServer (no UI, no ActiveView).
            _server = addInSiteObject.InventorServer;
            _automation = new GearAutomation(_server);
        }

        public void Deactivate()
        {
            if (_server != null) Marshal.ReleaseComObject(_server);
            _server = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ExecuteCommand(int commandID) { }

        // DA calls Automation.Run(doc) or Automation.RunWithArguments(doc, map) by reflection.
        public object? Automation => _automation;
    }
}

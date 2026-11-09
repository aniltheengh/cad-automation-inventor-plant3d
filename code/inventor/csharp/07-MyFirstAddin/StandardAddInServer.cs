// StandardAddInServer.cs - entry point Inventor calls when it loads the add-in.
// Lifecycle: Inventor reads MyFirstAddin.addin -> loads MyFirstAddin.dll -> creates this class
// (matched by GUID) -> Activate(). On exit / unload -> Deactivate().
using System.Runtime.InteropServices;
using Inventor;
using MyFirstAddin.Commands;

namespace MyFirstAddin
{
    [Guid("2ECD787D-3623-49EC-8830-C0052D21C63A")]
    [ComVisible(true)]
    public class StandardAddInServer : ApplicationAddInServer
    {
        public const string ClientId = "{2ECD787D-3623-49EC-8830-C0052D21C63A}";
        internal static Inventor.Application App = null!;

        private readonly List<AddinCommand> _commands = new();
        private UserInterfaceEvents? _uiEvents;
        private const string TabId = "id_Tab_CadAutomation";
        private const string PanelId = "id_Panel_CadAutomation_Tools";

        public void Activate(ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            App = addInSiteObject.Application;
            try
            {
                // 1. Button definitions must be created on EVERY load (they are not persisted).
                _commands.Add(new ExportPdfCommand());
                _commands.Add(new PartInfoCommand());
                _commands.Add(new ParameterEditorCommand());
                foreach (var c in _commands) c.Create(App);

                // 2. Ribbon controls.
                BuildRibbon();

                // 3. If the user does "Reset Ribbon", Inventor drops our controls - rebuild them.
                _uiEvents = App.UserInterfaceManager.UserInterfaceEvents;
                _uiEvents.OnResetRibbonInterface += OnResetRibbon;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("MyFirstAddin failed to load:\n" + ex, "MyFirstAddin");
            }
        }

        private void OnResetRibbon(NameValueMap context) => BuildRibbon();

        private void BuildRibbon()
        {
            // Ribbon names: "ZeroDoc" (no document open), "Part", "Assembly", "Drawing", "Presentation"
            foreach (string ribbonName in new[] { "Part", "Assembly", "Drawing" })
            {
                Ribbon ribbon = App.UserInterfaceManager.Ribbons[ribbonName];
                RibbonTab tab = FindTab(ribbon, TabId)
                                ?? ribbon.RibbonTabs.Add("CAD Automation", TabId, ClientId, "", false, false);
                RibbonPanel panel = FindPanel(tab, PanelId)
                                    ?? tab.RibbonPanels.Add("Tools", PanelId, ClientId, "", false);
                foreach (var c in _commands)
                    if (c.AppliesTo(ribbonName) && !HasControl(panel, c.InternalName))
                        panel.CommandControls.AddButton(c.Definition, true, true, "", false);
            }
        }

        private static RibbonTab? FindTab(Ribbon r, string id)
        { foreach (RibbonTab t in r.RibbonTabs) if (t.InternalName == id) return t; return null; }
        private static RibbonPanel? FindPanel(RibbonTab t, string id)
        { foreach (RibbonPanel p in t.RibbonPanels) if (p.InternalName == id) return p; return null; }
        private static bool HasControl(RibbonPanel p, string id)
        { foreach (CommandControl c in p.CommandControls) if (c.InternalName == id) return true; return false; }

        public void Deactivate()
        {
            if (_uiEvents != null) _uiEvents.OnResetRibbonInterface -= OnResetRibbon;
            foreach (var c in _commands) c.Dispose();
            _commands.Clear();
            // release COM references so Inventor can shut down cleanly
            Marshal.ReleaseComObject(App);
            App = null!;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        // Obsolete in the API but still part of the interface
        public void ExecuteCommand(int commandID) { }

        // Expose an object here if other add-ins / EXEs should call into this add-in:
        //   app.ApplicationAddIns.ItemById["{GUID}"].Automation
        public object? Automation => null;
    }
}

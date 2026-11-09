using System.Windows.Forms;
using Inventor;

namespace MyFirstAddin.Commands
{
    /// <summary>Small grid to edit user parameters of the active part/assembly, then Update.</summary>
    public sealed class ParameterEditorCommand : AddinCommand
    {
        public override string DisplayName => "Param Editor";
        public override string InternalName => "CadAutomation_ParamEditor";
        public override string Tooltip => "Edit user parameters in a simple grid";
        public override string IconText => "=";
        public override bool AppliesTo(string ribbonName) => ribbonName is "Part" or "Assembly";

        protected override void Execute()
        {
            Document doc = App.ActiveEditDocument;
            Parameters ps = doc switch
            {
                PartDocument p => p.ComponentDefinition.Parameters,
                AssemblyDocument a => a.ComponentDefinition.Parameters,
                _ => throw new InvalidOperationException("Open a part or assembly.")
            };

            using var form = new Form { Text = "User parameters - " + doc.DisplayName, Width = 560, Height = 420, StartPosition = FormStartPosition.CenterScreen };
            var grid = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            grid.Columns.Add("Name", "Name"); grid.Columns[0].ReadOnly = true;
            grid.Columns.Add("Expression", "Expression");
            grid.Columns.Add("Units", "Units"); grid.Columns[2].ReadOnly = true;
            foreach (UserParameter up in ps.UserParameters)
                grid.Rows.Add(up.Name, up.Expression, up.get_Units());
            var ok = new Button { Text = "Apply + Update", Dock = DockStyle.Bottom, Height = 36 };
            ok.Click += (_, _) => { form.DialogResult = DialogResult.OK; form.Close(); };
            form.Controls.Add(grid); form.Controls.Add(ok);

            // Parent the form to Inventor's main window so it stays on top of Inventor
            if (form.ShowDialog(new WindowWrapper((IntPtr)App.MainFrameHWND)) != DialogResult.OK) return;

            // One undo step for all changes
            Transaction tx = App.TransactionManager.StartTransaction((_Document)doc, "Edit user parameters");
            try
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    UserParameter up = ps.UserParameters[(string)row.Cells[0].Value];
                    string expr = Convert.ToString(row.Cells[1].Value) ?? "";
                    if (expr != up.Expression) up.Expression = expr;
                }
                doc.Update();
                tx.End();
            }
            catch
            {
                tx.Abort();
                throw;
            }
        }

        private sealed class WindowWrapper : IWin32Window
        {
            public WindowWrapper(IntPtr h) => Handle = h;
            public IntPtr Handle { get; }
        }
    }
}

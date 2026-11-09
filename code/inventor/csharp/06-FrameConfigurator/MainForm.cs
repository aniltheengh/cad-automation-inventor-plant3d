// MainForm.cs - simple WinForms front end, built in code (no designer file needed).
using CadAutomation.Shared;

namespace FrameConfigurator
{
    public sealed class MainForm : Form
    {
        private readonly NumericUpDown _l = Num(1200, 100, 6000), _w = Num(600, 100, 3000), _h = Num(750, 100, 3000),
                                       _a = Num(40, 10, 200), _t = Num(3, 1, 20);
        private readonly TextBox _name = new() { Text = "FRAME-001", Width = 200 };
        private readonly TextBox _folder = new() { Text = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "Frames"), Width = 300 };
        private readonly TextBox _log = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Height = 140, Width = 420 };

        public MainForm()
        {
            Text = "Frame Configurator (Inventor)";
            AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink; Padding = new Padding(10);
            var grid = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Dock = DockStyle.Fill };
            void Row(string label, Control c) { grid.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }); grid.Controls.Add(c); }
            Row("Length L (mm)", _l); Row("Width W (mm)", _w); Row("Height H (mm)", _h);
            Row("Tube size A (mm)", _a); Row("Wall t (mm)", _t); Row("Name / part number", _name); Row("Output folder", _folder);
            var go = new Button { Text = "Build in Inventor", AutoSize = true };
            go.Click += (_, _) => BuildFrame();
            grid.Controls.Add(go); grid.Controls.Add(new Label());
            grid.Controls.Add(_log); grid.SetColumnSpan(_log, 2);
            Controls.Add(grid);
        }

        private static NumericUpDown Num(decimal v, decimal min, decimal max) =>
            new() { Minimum = min, Maximum = max, Value = v, DecimalPlaces = 1, Width = 100 };

        private void BuildFrame()
        {
            UseWaitCursor = true;
            try
            {
                var app = InventorConnector.Connect(startIfNotRunning: true);
                var spec = new FrameSpec((double)_l.Value, (double)_w.Value, (double)_h.Value, (double)_a.Value, (double)_t.Value,
                                         System.IO.Path.Combine(_folder.Text, _name.Text), _name.Text);
                var log = new Progress<string>(s => _log.AppendText(s + Environment.NewLine));
                string file = new FrameBuilder(app).Build(spec, log);
                _log.AppendText("DONE: " + file + Environment.NewLine);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Build failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { UseWaitCursor = false; }
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            System.Windows.Forms.Application.Run(new MainForm());
        }
    }
}

// AddinCommand.cs - base class: one ButtonDefinition + its click handler.
using System.Drawing;
using System.Windows.Forms;
using Inventor;

namespace MyFirstAddin.Commands
{
    public abstract class AddinCommand : IDisposable
    {
        public abstract string DisplayName { get; }
        public abstract string InternalName { get; }   // must be unique across ALL add-ins -> prefix it
        public abstract string Tooltip { get; }
        public virtual string IconText => DisplayName.Substring(0, 1);
        public virtual bool AppliesTo(string ribbonName) => true;

        public ButtonDefinition Definition { get; private set; } = null!;
        protected Inventor.Application App { get; private set; } = null!;

        public void Create(Inventor.Application app)
        {
            App = app;
            // Icons: Inventor wants IPictureDisp (16x16 small, 32x32 large). We draw simple letter icons
            // so the sample needs no image files; replace with your own PNGs (embedded resources) later.
            object small = PictureConverter.ToIPictureDisp(MakeIcon(16, IconText));
            object large = PictureConverter.ToIPictureDisp(MakeIcon(32, IconText));
            Definition = app.CommandManager.ControlDefinitions.AddButtonDefinition(
                DisplayName, InternalName, CommandTypesEnum.kQueryOnlyCmdType,
                StandardAddInServer.ClientId, Tooltip, Tooltip, small, large,
                ButtonDisplayEnum.kAlwaysDisplayText);
            Definition.OnExecute += OnExecuteSafe;
        }

        private void OnExecuteSafe(NameValueMap context)
        {
            // An unhandled exception inside an event handler can take Inventor down - always catch.
            try { Execute(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, DisplayName, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        protected abstract void Execute();

        private static Bitmap MakeIcon(int size, string text)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.Clear(System.Drawing.Color.FromArgb(0, 102, 204));
            using var f = new Font("Segoe UI", size * 0.55f, FontStyle.Bold, GraphicsUnit.Pixel);
            var sz = g.MeasureString(text, f);
            g.DrawString(text, f, Brushes.White, (size - sz.Width) / 2, (size - sz.Height) / 2);
            return bmp;
        }

        public void Dispose()
        {
            if (Definition != null)
            {
                Definition.OnExecute -= OnExecuteSafe;
                try { Definition.Delete(); } catch { /* Inventor shutting down */ }
            }
        }
    }

    /// <summary>Image -> stdole.IPictureDisp through the protected AxHost helper (works in .NET 8 WinForms).</summary>
    internal sealed class PictureConverter : AxHost
    {
        private PictureConverter() : base(string.Empty) { }
        public static object ToIPictureDisp(Image img) => GetIPictureDispFromPicture(img);
    }
}

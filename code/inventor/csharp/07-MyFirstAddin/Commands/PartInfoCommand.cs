using System.Text;
using System.Windows.Forms;
using Inventor;

namespace MyFirstAddin.Commands
{
    /// <summary>Shows part number, material, mass, volume, bounding box of the active part.</summary>
    public sealed class PartInfoCommand : AddinCommand
    {
        public override string DisplayName => "Part Info";
        public override string InternalName => "CadAutomation_PartInfo";
        public override string Tooltip => "Mass, material and size of the active part";
        public override string IconText => "i";
        public override bool AppliesTo(string ribbonName) => ribbonName is "Part" or "Assembly";

        protected override void Execute()
        {
            Document doc = App.ActiveEditDocument;     // in-place edit inside an assembly -> the edited part
            // NOTE: in the interop, PartComponentDefinition does NOT derive from ComponentDefinition
            // (COM interfaces are flattened), so branch per document type.
            (MassProperties mp, Box box) = doc switch
            {
                PartDocument p => (p.ComponentDefinition.MassProperties, p.ComponentDefinition.RangeBox),
                AssemblyDocument a => (a.ComponentDefinition.MassProperties, a.ComponentDefinition.RangeBox),
                _ => throw new InvalidOperationException("Open a part or assembly.")
            };
            UnitsOfMeasure uom = doc.UnitsOfMeasure;
            double dx = box.MaxPoint.X - box.MinPoint.X, dy = box.MaxPoint.Y - box.MinPoint.Y, dz = box.MaxPoint.Z - box.MinPoint.Z;
            string L(double cm) => uom.GetStringFromValue(cm, UnitsTypeEnum.kDefaultDisplayLengthUnits);

            var sb = new StringBuilder();
            sb.AppendLine("File        : " + doc.DisplayName);
            sb.AppendLine("Part Number : " + doc.PropertySets["Design Tracking Properties"]["Part Number"].Value);
            if (doc is PartDocument part)
                sb.AppendLine("Material    : " + part.ActiveMaterial.DisplayName);
            sb.AppendLine("Mass        : " + uom.GetStringFromValue(mp.Mass, UnitsTypeEnum.kDefaultDisplayMassUnits));
            sb.AppendLine($"Volume      : {mp.Volume:F2} cm^3");
            sb.AppendLine($"Size (X,Y,Z): {L(dx)} x {L(dy)} x {L(dz)}");
            sb.AppendLine("Accuracy    : " + mp.Accuracy);
            MessageBox.Show(sb.ToString(), "Part Info");
        }
    }
}

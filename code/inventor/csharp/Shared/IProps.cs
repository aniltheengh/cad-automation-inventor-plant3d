// IProps.cs - friendly-name map for Inventor iProperties + safe get/set helpers.
// PropertySet internal names are stable in every language; display names are localised.
using Inventor;

namespace CadAutomation.Shared
{
    public static class IProps
    {
        public const string Summary = "Inventor Summary Information";              // Title, Author, Revision Number, Comments...
        public const string DocSummary = "Inventor Document Summary Information";  // Company, Category, Manager
        public const string Design = "Design Tracking Properties";                 // Part Number, Description, Stock Number, Vendor...
        public const string Custom = "Inventor User Defined Properties";           // user / custom iProperties

        /// <summary>Column name -> (property set, property name). Anything else = custom property.</summary>
        public static readonly IReadOnlyDictionary<string, (string Set, string Name)> Standard =
            new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
            {
                ["Part Number"] = (Design, "Part Number"),
                ["Description"] = (Design, "Description"),
                ["Stock Number"] = (Design, "Stock Number"),
                ["Vendor"] = (Design, "Vendor"),
                ["Designer"] = (Design, "Designer"),
                ["Project"] = (Design, "Project"),
                ["Engineer"] = (Design, "Engineer"),
                ["Cost Center"] = (Design, "Cost Center"),
                ["Material"] = (Design, "Material"),                 // read-only here; set via PartDocument.ActiveMaterial
                ["Title"] = (Summary, "Title"),
                ["Subject"] = (Summary, "Subject"),
                ["Author"] = (Summary, "Author"),
                ["Revision Number"] = (Summary, "Revision Number"),
                ["Comments"] = (Summary, "Comments"),
                ["Company"] = (DocSummary, "Company"),
                ["Category"] = (DocSummary, "Category"),
            };

        public static string Get(Document doc, string column)
        {
            try
            {
                if (Standard.TryGetValue(column, out var k))
                    return Convert.ToString(doc.PropertySets[k.Set][k.Name].Value) ?? "";
                return Convert.ToString(doc.PropertySets[Custom][column].Value) ?? "";
            }
            catch (System.Runtime.InteropServices.COMException) { return ""; }  // custom prop missing
        }

        /// <summary>Sets a value; creates a custom property if needed. Returns true if something changed.</summary>
        public static bool Set(Document doc, string column, string value)
        {
            if (string.Equals(column, "Material", StringComparison.OrdinalIgnoreCase)) return false;
            if (Get(doc, column) == value) return false;
            if (Standard.TryGetValue(column, out var k))
            {
                doc.PropertySets[k.Set][k.Name].Value = value;
                return true;
            }
            PropertySet custom = doc.PropertySets[Custom];
            try { custom[column].Value = value; }
            catch (System.Runtime.InteropServices.COMException) { custom.Add(value, column); }
            return true;
        }

        public static IEnumerable<string> CustomNames(Document doc)
        {
            foreach (Property p in doc.PropertySets[Custom]) yield return p.Name;
        }
    }
}

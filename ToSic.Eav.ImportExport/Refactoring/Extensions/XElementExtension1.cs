using System.Linq;
using System.Xml.Linq;

namespace ToSic.Eav.ImportExport.Refactoring.Extensions
{
    public static class XElementExtension1
    {
        public static bool HasChildren(this XElement element) => element.Elements().Any();

        /// <summary>
        /// Apend an element to this.
        /// </summary>
        public static void Append(this XElement element, XName name, object value)
        {
            element.Add(new XElement(name, value));
        }
    
        public static string GetChildElementValue(this XElement element, string childElementName)
        {
            var childElement = element.Element(childElementName);

            return childElement?.Value;
        }
    }
}
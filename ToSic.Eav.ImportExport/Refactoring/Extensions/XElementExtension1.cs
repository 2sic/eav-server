using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.ImportExport.Refactoring.Options;

namespace ToSic.Eav.ImportExport.Refactoring.Extensions
{
    public static class XElementExtension1
    {
        public static bool HasChildren(this XElement element)
        {
            return element.Elements().Count() > 0;
        }

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
            if (childElement == null)
                return null;

            return childElement.Value;
        }
    }
}
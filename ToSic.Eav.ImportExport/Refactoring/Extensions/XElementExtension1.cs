using System.Xml.Linq;

namespace ToSic.Eav.ImportExport.Refactoring.Extensions
{
    public static class XElementExtension1
    {
        /// <summary>
        /// Apend an element to this.
        /// </summary>
        public static void Append(this XElement element, XName name, object value)
        {
            element.Add(new XElement(name, value));
        }
    }
}
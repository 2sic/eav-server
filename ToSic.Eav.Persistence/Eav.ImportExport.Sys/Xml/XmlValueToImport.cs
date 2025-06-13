using System.Xml.Linq;

namespace ToSic.Eav.ImportExport.Sys.Xml;

internal class XmlValueToImport
{
    public required XElement XmlValue;
    public required List<ILanguage> Dimensions;
}
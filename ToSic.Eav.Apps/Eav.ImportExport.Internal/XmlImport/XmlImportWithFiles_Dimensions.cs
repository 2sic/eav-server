using System.Xml.Linq;
using ToSic.Eav.ImportExport.Internal.Xml;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.ImportExport.Internal;

partial class XmlImportWithFiles
{

    private static List<DimensionDefinition> BuildSourceDimensionsList(XElement xmlSource)
    {
        var sDimensions =
            xmlSource.Element(XmlConstants.Header)?
                .Element(XmlConstants.DimensionDefinition)?
                .Elements(XmlConstants.DimensionDefElement)
                .Select(p => new DimensionDefinition()
                {
                    DimensionId = int.Parse(p.Attribute(XmlConstants.DimId).Value),
                    Name = p.Attribute(XmlConstants.Name).Value,
                    Key = p.Attribute(XmlConstants.CultureSysKey).Value,
                    EnvironmentKey = p.Attribute(XmlConstants.CultureExtKey).Value,
                    Active = bool.Parse(p.Attribute(XmlConstants.CultureIsActiveAttrib).Value)
                }).ToList();
        return sDimensions;
    }

		

}
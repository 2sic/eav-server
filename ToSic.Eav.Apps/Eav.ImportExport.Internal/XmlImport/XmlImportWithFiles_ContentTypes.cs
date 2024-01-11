using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;
using ToSic.Lib.Logging;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.ImportExport.Internal;

partial class XmlImportWithFiles
{

    private List<IContentType> GetImportContentTypes(IReadOnlyCollection<XElement> list) => Log.Func($"items: {list.Count}", () =>
    {
        // Loop through AttributeSets
        var importTypes = list
            .Select(BuildContentTypeFromXml)
            .Where(t => t != null)
            .ToList();

        return (importTypes, $"found {importTypes.Count}");
    });

    private IContentType BuildContentTypeFromXml(XElement xmlContentType) => Log.Func(l =>
    {
        var ctElement = xmlContentType.Element(XmlConstants.Attributes);
        var typeName = xmlContentType.Attribute(XmlConstants.Name).Value;

        var attributes = new List<IContentTypeAttribute>();
        if (ctElement != null)
        {
            // Figure out which one is the title
            var set = ctElement.Elements(XmlConstants.Attribute).Select(xmlField =>
            {
                if (!bool.Parse(xmlField.Attribute(XmlConstants.IsTitle).Value))
                    return new
                    {
                        XmlElement = xmlField,
                        IsTitle = false
                    };

                l.A("set title on this attribute");
                return new
                {
                    XmlElement = xmlField,
                    IsTitle = true
                };
            }).ToList();

            // If neither is the title, make sure the first one is
            if (set.Any() && !set.Any(a => a.IsTitle))
                set = set.Select((s, i) => i == 0 ? new { s.XmlElement, IsTitle = true } : s).ToList();


            foreach (var s in set)
            {
                var xmlField = s.XmlElement;
                var name = xmlField.Attribute(XmlConstants.Static).Value;
                var fieldTypeName = xmlField.Attribute(XmlConstants.EntityTypeAttribute).Value;

                var xmlMetadata = xmlField.Elements(XmlConstants.Entity).ToList();
                var attributeMetadata = BuildEntities(xmlMetadata, (int)TargetTypes.Attribute);
                var attribute = Services.MultiBuilder.Value.TypeAttributeBuilder.Create(
                    appId: AppId,
                    name: name,
                    type: ValueTypeHelpers.Get(fieldTypeName),
                    isTitle: s.IsTitle,
                    metadataItems: attributeMetadata
                );
                attributes.Add(attribute);

                l.A($"Attribute: {name} ({fieldTypeName}) with {xmlMetadata.Count} metadata items");

            }
        }


        #region check for shared type and if it's allowed

        var isSharedType = xmlContentType.Attributes(XmlConstants.AlwaysShareConfig).Any() &&
                           bool.Parse(xmlContentType.Attribute(XmlConstants.AlwaysShareConfig).Value);

        if (isSharedType & !AllowUpdateOnSharedTypes)
        {
            l.A("trying to update a shared type, but not allowed");
            return (null, "error");
        }

        #endregion

        // create ContentType
        var ct = Services.MultiBuilder.Value.ContentType.Create(
            appId: AppId,
            id: 0,
            name: typeName,
            nameId: xmlContentType.Attribute(XmlConstants.Static).Value,
            scope: xmlContentType.Attributes(XmlConstants.Scope).Any()
                ? xmlContentType.Attribute(XmlConstants.Scope).Value
                : base.Services.Environment.FallbackContentTypeScope,
            attributes: attributes,
            isAlwaysShared: AllowUpdateOnSharedTypes && isSharedType,
            onSaveSortAttributes: xmlContentType.Attributes(XmlConstants.SortAttributes).Any() &&
                                  bool.Parse(xmlContentType.Attribute(XmlConstants.SortAttributes).Value),
            onSaveUseParentStaticName: xmlContentType.Attributes(XmlConstants.AttributeSetParentDef).Any()
                ? xmlContentType.Attribute(XmlConstants.AttributeSetParentDef).Value
                : ""
        );


        return (ct, "ok");
    });

}
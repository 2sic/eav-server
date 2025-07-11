using System.Xml.Linq;
using ToSic.Eav.Data.Sys.Values;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.ImportExport.Sys.Xml;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.ImportExport.Sys.XmlImport;

partial class XmlImportWithFiles
{

    private List<IContentType> GetImportContentTypes(IReadOnlyCollection<XElement> list)
    {
        var l = Log.Fn<List<IContentType>>($"for {list.Count} items");
        // Loop through AttributeSets
        var importTypes = list
            .Select(BuildContentTypeFromXml)
            .Where(t => t != null)
            .Cast<IContentType>()
            .ToList();

        return l.Return(importTypes, $"found {importTypes.Count}");
    }

    private IContentType? BuildContentTypeFromXml(XElement xmlContentType)
    {
        var l = LogSummary.Fn<IContentType>();
        var ctElement = xmlContentType.Element(XmlConstants.Attributes);
        var typeName = xmlContentType.Attribute(XmlConstants.Name)!.Value;

        var attributes = new List<IContentTypeAttribute>();
        var metadataCount = 0;
        if (ctElement != null)
        {
            // Figure out which one is the title
            var set = ctElement
                .Elements(XmlConstants.Attribute)
                .Select(xmlField =>
                {
                    var isTitle = bool.Parse(xmlField.Attribute(XmlConstants.IsTitle)?.Value ?? "false");
                    l.If(isTitle).A("set title on this attribute");
                    return new { xmlField, isTitle };
                })
                .ToList();

            // If neither is the title, make sure the first one is
            if (set.Any() && !set.Any(a => a.isTitle))
                set = set
                    .Select((s, i) => i == 0 ? s with { isTitle = true } : s)
                    .ToList();


            foreach (var s in set)
            {
                var xmlField = s.xmlField;
                var nameId = xmlField.Attribute(XmlConstants.Static)?.Value
                    ?? throw new ArgumentNullException(XmlConstants.Static, @"Can't find the static name.");
                var fieldTypeName = xmlField.Attribute(XmlConstants.EntityTypeAttribute)?.Value;

                var xmlMetadata = xmlField
                    .Elements(XmlConstants.Entity)
                    .ToList();
                var attributeMetadata = BuildEntities(xmlMetadata, (int)TargetTypes.Attribute);
                metadataCount += attributeMetadata.Count;
                // #SharedFieldDefinition
                Guid? guid = null;
                if (Guid.TryParse(xmlField.Attribute(XmlConstants.Guid)?.Value, out var result))
                    guid = result;
                var sysSettings = JsonDeserializeAttribute.SysSettings(nameId, xmlField.Attribute(XmlConstants.SysSettings)?.Value, Log);

                var attribute = Services.MultiBuilder.Value.TypeAttributeBuilder
                    .Create(
                        appId: AppId,
                        name: nameId,
                        type: ValueTypeHelpers.Get(fieldTypeName),
                        isTitle: s.isTitle,
                        metadataItems: attributeMetadata,
                        guid: guid,
                        sysSettings: sysSettings
                    );
                attributes.Add(attribute);

                l.A($"Attribute: {nameId} ({fieldTypeName}) with {xmlMetadata.Count} metadata items");

            }

        }



        #region check for shared type and if it's allowed

        var isSharedType = xmlContentType.Attributes(XmlConstants.AlwaysShareConfig).Any()
                           && bool.Parse(xmlContentType.Attribute(XmlConstants.AlwaysShareConfig)!.Value);

        if (isSharedType & !AllowUpdateOnSharedTypes)
            return l.ReturnNull("error, trying to update a shared type, but not allowed");

        #endregion

        // create ContentType
        var scope = xmlContentType.Attributes(XmlConstants.Scope).Any()
            ? xmlContentType.Attribute(XmlConstants.Scope)?.Value
              ?? Services.Environment.FallbackContentTypeScope
            : Services.Environment.FallbackContentTypeScope;
        var ct = Services.MultiBuilder.Value.ContentType.Create(
            appId: AppId,
            id: 0,
            name: typeName,
            nameId: xmlContentType.Attribute(XmlConstants.Static)!.Value,
            scope: scope,
            attributes: attributes,
            isAlwaysShared: AllowUpdateOnSharedTypes && isSharedType,
            onSaveSortAttributes: xmlContentType.Attributes(XmlConstants.SortAttributes).Any() &&
                                  bool.Parse(xmlContentType.Attribute(XmlConstants.SortAttributes)!.Value),
            onSaveUseParentStaticName: xmlContentType.Attributes(XmlConstants.AttributeSetParentDef).Any()
                ? xmlContentType.Attribute(XmlConstants.AttributeSetParentDef)!.Value
                : ""
        );


        return l.Return(ct, $"ok, metadataCount:{metadataCount}");
    }

}
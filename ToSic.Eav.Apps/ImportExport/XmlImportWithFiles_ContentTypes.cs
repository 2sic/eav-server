using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.Apps.ImportExport
{
    public partial class XmlImportWithFiles
    {

        private List<IContentType> GetImportContentTypes(IReadOnlyCollection<XElement> list) => Log.Func($"items: {list.Count}", () =>
        {
            //var list = xmlContentTypes.ToList();
            //Log.A($"items: {list.Count}");
            //var importTypes = new List<IContentType>();
            //foreach (var contentType in list)
            //{
            //    var ct = BuildContentTypeFromXml(contentType);
            //    if (ct != null)
            //        importTypes.Add(ct);
            //}

            // Loop through AttributeSets
            var importTypes = list
                .Select(BuildContentTypeFromXml)
                .Where(t => t != null)
                .ToList();

            return (importTypes, $"found {importTypes.Count}");
        });

	    private IContentType BuildContentTypeFromXml(XElement xmlContentType)
	    {
	        var ctElement = xmlContentType.Element(XmlConstants.Attributes);
	        var typeName = xmlContentType.Attribute(XmlConstants.Name).Value;
	        var wrapLog = Log.Fn<IContentType>(typeName);

	        var attributes = new List<IContentTypeAttribute>();
            if (ctElement != null)
            {
                foreach (var xmlField in ctElement.Elements(XmlConstants.Attribute))
                {
                    var name = xmlField.Attribute(XmlConstants.Static).Value;
                    var fieldTypeName = xmlField.Attribute(XmlConstants.EntityTypeAttribute).Value;
                    var attribute = new ContentTypeAttribute(AppId, name, fieldTypeName, attributeMetadata: new List<IEntity>
                        {
                            base.Services.CtAttribBuilder.Value.GenerateAttributeMetadata(AppId, null, null, null,
                                string.Empty, null)
                        });
                    var md = xmlField.Elements(XmlConstants.Entity).ToList();
                    ((IMetadataInternals)attribute.Metadata).Use(BuildEntities(md, (int)TargetTypes.Attribute));
                    attributes.Add(attribute);

                    Log.A($"Attribute: {name} ({fieldTypeName}) with {md.Count} metadata items");

                    // Set Title Attribute
                    if (bool.Parse(xmlField.Attribute(XmlConstants.IsTitle).Value))
                    {
                        Log.A("set title on this attribute");
                        attribute.IsTitle = true;
                    }
                }

                // check if it's normal (not a ghost) but still missing a title
                if (attributes.Any() && !attributes.Any(a => a.IsTitle))
                    (attributes.First() as ContentTypeAttribute).IsTitle = true;
            }

            
            #region check for shared type and if it's allowed

            var isSharedType = xmlContentType.Attributes(XmlConstants.AlwaysShareConfig).Any() &&
                           bool.Parse(xmlContentType.Attribute(XmlConstants.AlwaysShareConfig).Value);

            if(isSharedType & !AllowUpdateOnSharedTypes)
            {
                Log.A("trying to update a shared type, but not allowed");
                return wrapLog.ReturnNull("error"); 
            }

            #endregion
            
            // create ContentType
            var ct = new ContentType(
                appId: AppId,
                name: typeName,
                nameId: xmlContentType.Attribute(XmlConstants.Static).Value,
                scope: xmlContentType.Attributes(XmlConstants.Scope).Any()
                    ? xmlContentType.Attribute(XmlConstants.Scope).Value
                    : base.Services.Environment.FallbackContentTypeScope,
                attributes: attributes,
                alwaysShareConfiguration: AllowUpdateOnSharedTypes && isSharedType,
                onSaveSortAttributes: xmlContentType.Attributes(XmlConstants.SortAttributes).Any() &&
                                      bool.Parse(xmlContentType.Attribute(XmlConstants.SortAttributes).Value),
                onSaveUseParentStaticName: xmlContentType.Attributes(XmlConstants.AttributeSetParentDef).Any()
	                ? xmlContentType.Attribute(XmlConstants.AttributeSetParentDef).Value
	                : ""
	        );


         //   ct.SetImportParameters(
	        //    scope: xmlContentType.Attributes(XmlConstants.Scope).Any()
	        //        ? xmlContentType.Attribute(XmlConstants.Scope).Value
	        //        : base.Services.Environment.FallbackContentTypeScope,
	        //    nameId: xmlContentType.Attribute(XmlConstants.Static).Value,
         //       // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
         //       // description: xmlContentType.Attribute(XmlConstants.Description).Value,
	        //    AllowUpdateOnSharedTypes && isSharedType
	        //);
	        return wrapLog.ReturnAsOk(ct);
        }

	}

}
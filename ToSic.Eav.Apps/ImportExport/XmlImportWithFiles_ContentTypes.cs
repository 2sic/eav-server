using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Metadata;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.Apps.ImportExport
{
    public partial class XmlImportWithFiles
    {

		private List<IContentType> GetImportContentTypes(IEnumerable<XElement> xmlContentTypes)
		{
            var wrap = Log.Call();
            var list = xmlContentTypes.ToList();
            Log.Add($"items: {list.Count}");

            var importAttributeSets = new List<IContentType>();

			// Loop through AttributeSets
			foreach (var attributeSet in list)
			{
			    var ct = BuildContentTypeFromXml(attributeSet);
                if (ct != null)
                    importAttributeSets.Add(ct);
            }

		    wrap($"found {importAttributeSets.Count}");
			return importAttributeSets;
		}

	    private IContentType BuildContentTypeFromXml(XElement xmlContentType)
	    {
	        var ctElement = xmlContentType.Element(XmlConstants.Attributes);
	        var typeName = xmlContentType.Attribute(XmlConstants.Name).Value;
	        var wrapLog = Log.Call(typeName);

	        var attributes = new List<IContentTypeAttribute>();
            if (ctElement != null)
            {
                foreach (var xmlField in ctElement.Elements(XmlConstants.Attribute))
                {
                    var name = xmlField.Attribute(XmlConstants.Static).Value;
                    var fieldTypeName = xmlField.Attribute(XmlConstants.EntityTypeAttribute).Value;
                    var attribute = new ContentTypeAttribute(AppId, name, fieldTypeName, attributeMetadata: new List<IEntity>
                        {
                            AttDefBuilder.GenerateAttributeMetadata(Deps.GlobalApp, AppId, null, null, null,
                                string.Empty, null)
                        });
                    var md = xmlField.Elements(XmlConstants.Entity).ToList();
                    ((IMetadataInternals)attribute.Metadata).Use(BuildEntities(md, (int)TargetTypes.Attribute));
                    attributes.Add(attribute);

                    Log.Add($"Attribute: {name} ({fieldTypeName}) with {md.Count} metadata items");

                    // Set Title Attribute
                    if (bool.Parse(xmlField.Attribute(XmlConstants.IsTitle).Value))
                    {
                        Log.Add("set title on this attribute");
                        attribute.IsTitle = true;
                    }
                }

                // check if it's normal (not a ghost) but still missing a title
                if (attributes.Any() && !attributes.Any(a => a.IsTitle))
                    (attributes.First() as ContentTypeAttribute).IsTitle = true;
            }

            // create ContentType
	        var ct = new ContentType(AppId, typeName)
	        {
	            Attributes = attributes,
	            OnSaveUseParentStaticName = xmlContentType.Attributes(XmlConstants.AttributeSetParentDef).Any()
	                ? xmlContentType.Attribute(XmlConstants.AttributeSetParentDef).Value
	                : "",
	            OnSaveSortAttributes = xmlContentType.Attributes(XmlConstants.SortAttributes).Any() &&
	                                   bool.Parse(xmlContentType.Attribute(XmlConstants.SortAttributes).Value)
	        };

            #region check for shared type and if it's allowed
            var isSharedType = xmlContentType.Attributes(XmlConstants.AlwaysShareConfig).Any() &&
                           bool.Parse(xmlContentType.Attribute(XmlConstants.AlwaysShareConfig).Value);

            if(isSharedType & !AllowUpdateOnSharedTypes)
            {
                Log.Add("trying to update a shared type, but not allowed");
                wrapLog("error");
                return null;
            }
            #endregion

            ct.SetImportParameters(
	            scope: xmlContentType.Attributes(XmlConstants.Scope).Any()
	                ? xmlContentType.Attribute(XmlConstants.Scope).Value
	                : Deps._environment.FallbackContentTypeScope,
	            staticName: xmlContentType.Attribute(XmlConstants.Static).Value,
	            description: xmlContentType.Attribute(XmlConstants.Description).Value,
	            AllowUpdateOnSharedTypes && isSharedType
	        );
	        wrapLog("ok");
	        return ct;
	    }

	}

}
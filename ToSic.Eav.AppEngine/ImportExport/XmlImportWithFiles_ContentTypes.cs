using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.Apps.ImportExport
{
    public partial class XmlImportWithFiles
    {

		private List<ContentType> GetImportContentTypes(IEnumerable<XElement> xAttributeSets)
		{
            var wrap = Log.Call("GetImportContentTypes", "start");
            Log.Add($"items: {xAttributeSets.Count()}");

            var importAttributeSets = new List<ContentType>();

			// Loop through AttributeSets
			foreach (var attributeSet in xAttributeSets)
			{
			    var ct = BuildContentTypeFromXml(attributeSet);
			    importAttributeSets.Add(ct);
			}

		    wrap($"found {importAttributeSets.Count}");
			return importAttributeSets;
		}

	    private ContentType BuildContentTypeFromXml(XElement attributeSet)
	    {
	        var attributes = new List<IAttributeDefinition>();
	        var attsetElem = attributeSet.Element(XmlConstants.Attributes);
	        var typeName = attributeSet.Attribute(XmlConstants.Name).Value;
	        var wrap = Log.Call("BuildContentTypeFromXml", typeName);

	        if (attsetElem != null)
	            foreach (var xElementAttribute in attsetElem.Elements(XmlConstants.Attribute))
	            {
	                var attribute = new AttributeDefinition(AppId,
	                    xElementAttribute.Attribute(XmlConstants.Static).Value,
	                    null,
	                    xElementAttribute.Attribute(XmlConstants.EntityTypeAttribute).Value,
	                    null, null, null, null
	                );
	                attribute.Metadata.Use(GetImportEntities(xElementAttribute.Elements(XmlConstants.Entity),
	                    Constants.MetadataForAttribute));
	                attributes.Add(attribute);

	                // Set Title Attribute
	                if (bool.Parse(xElementAttribute.Attribute(XmlConstants.IsTitle).Value))
	                    attribute.IsTitle = true;
	            }
	        // check if it's normal (not a ghost) but still missing a title
	        if (attributes.Any() && !attributes.Any(a => a.IsTitle))
	            (attributes.First() as AttributeDefinition).IsTitle = true;

	        // create ContentType
	        var ct = new ContentType(AppId, typeName)
	        {
	            Attributes = attributes,
	            OnSaveUseParentStaticName = attributeSet.Attributes(XmlConstants.AttributeSetParentDef).Any()
	                ? attributeSet.Attribute(XmlConstants.AttributeSetParentDef).Value
	                : "",
	            OnSaveSortAttributes = attributeSet.Attributes(XmlConstants.SortAttributes).Any() &&
	                                   bool.Parse(attributeSet.Attribute(XmlConstants.SortAttributes).Value)
	        };

	        ct.SetImportParameters(
	            scope: attributeSet.Attributes(XmlConstants.Scope).Any()
	                ? attributeSet.Attribute(XmlConstants.Scope).Value
	                : _environment.FallbackContentTypeScope,
	            staticName: attributeSet.Attribute(XmlConstants.Static).Value,
	            description: attributeSet.Attribute(XmlConstants.Description).Value,
	            alwaysShareDef: AllowSystemChanges && attributeSet.Attributes(XmlConstants.AlwaysShareConfig).Any() &&
	                            Boolean.Parse(attributeSet.Attribute(XmlConstants.AlwaysShareConfig).Value)
	        );
	        wrap("ok");
	        return ct;
	    }

	}

}
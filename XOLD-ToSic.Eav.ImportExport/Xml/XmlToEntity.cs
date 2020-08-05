using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging;
using ToSic.Eav.Repositories;
using ToSic.Eav.Types;

namespace ToSic.Eav.ImportExport.Xml
{
	/// <summary>
	/// Import EAV Data from XML Format
	/// </summary>
	public class XmlToEntity: HasLog
	{
        private class TargetLanguageToSourceLanguage: DimensionDefinition
        {
            public List<DimensionDefinition> PriorizedDimensions = new List<DimensionDefinition>();
        }

        public int AppId { get; }
	    public XmlToEntity(int appId, List<DimensionDefinition> srcLanguages, int? srcDefLang, List<DimensionDefinition> envLanguages, string envDefLang, ILog parentLog): base("Imp.XmlEnt", parentLog, "init", "XmlToEntity")
	    {
	        AppId = appId;
            envLanguages = envLanguages.OrderByDescending(p => p.Matches(envDefLang)).ThenBy(p => p.EnvironmentKey).ToList();
	        _envLangs = PrepareTargetToSourceLanguageMapping(envLanguages, envDefLang, srcLanguages, srcDefLang);
	        _envDefLang = envDefLang;
            _srcDefLang = srcDefLang?.ToString();
	    }

	    private List<TargetLanguageToSourceLanguage> PrepareTargetToSourceLanguageMapping(List<DimensionDefinition> envLanguages, string envDefLang, List<DimensionDefinition> srcLanguages, int? srcDefLang)
	    {
	        var wrapLog = Log.Call($"Env has {envLanguages.Count} languages");
	        List<TargetLanguageToSourceLanguage> result;
            // if the environment doesn't have languages defined, we'll create a temp-entry for the main language to allow mapping
            if (envLanguages.Any())
            {
                result =
                    envLanguages.Select(el => new TargetLanguageToSourceLanguage
                    {
                        Active = el.Active,
                        EnvironmentKey = el.EnvironmentKey,
                        DimensionId = el.DimensionId,
                        PriorizedDimensions = FindPriorizedMatchingDimensions(el, envDefLang, srcLanguages, srcDefLang)
                    }).ToList();
            }
            else
            {
                var tempDimension = new DimensionDefinition()
                {
                    Active = true,
                    DimensionId = 0,
                    EnvironmentKey = envDefLang,
                    Name = "Default"
                };
                result = new List<TargetLanguageToSourceLanguage>
                {
                    new TargetLanguageToSourceLanguage
                    {
                        Active = true,
                        EnvironmentKey = envDefLang,
                        PriorizedDimensions = FindPriorizedMatchingDimensions(tempDimension, envDefLang, srcLanguages, srcDefLang)
                    }
                };
            }

	        wrapLog($"LanguageMap has {result.Count} items");
	        return result;
	    }

        private List<DimensionDefinition> FindPriorizedMatchingDimensions(DimensionDefinition targetLang, string envDefLang, List<DimensionDefinition> srcLangs, int? srcDefLang)
        {
            var languageMap = new List<DimensionDefinition>();
            
            // Add exact match source language, if exists
            var exactMatchSourceDimension = srcLangs.FirstOrDefault(p => targetLang.Matches(p.EnvironmentKey));
            if (exactMatchSourceDimension != null)
                languageMap.Add(exactMatchSourceDimension);

            // Add non-exact match language
            var unExactMatchSourceDimensions = srcLangs.Where(
                    sd =>
                        !targetLang.Matches(sd.EnvironmentKey) &&
                        sd.EnvironmentKey.StartsWith(targetLang.EnvironmentKey.ToLowerInvariant().Substring(0, 3)))
                .OrderByDescending(p => p.EnvironmentKey == envDefLang)
                .ThenByDescending(p => p.EnvironmentKey.Substring(0, 2) == p.EnvironmentKey.Substring(3, 2))
                .ThenBy(p => p.EnvironmentKey);
            languageMap.AddRange(unExactMatchSourceDimensions);

            // Add primary language, if current target is primary
            if (targetLang.Matches(envDefLang) && srcDefLang.HasValue)
            {
                var sourcePrimaryLanguage = srcLangs.FirstOrDefault(p => p.DimensionId == srcDefLang);
                if (sourcePrimaryLanguage != null && !languageMap.Contains(sourcePrimaryLanguage))
                    languageMap.Add(sourcePrimaryLanguage);
            }
            
            return languageMap;
        }

        //private readonly List<string> _relevantSrcLangsByPriority;
	    private readonly List<TargetLanguageToSourceLanguage> _envLangs;
	    private readonly string _envDefLang;
        private readonly string _srcDefLang;
        

        /// <summary>
        /// Returns an EAV import entity
        /// </summary>
        /// <param name="xEntity">xEntity to parse</param>
        /// <param name="metadataForFor"></param>
        public Entity BuildEntityFromXml(XElement xEntity, Metadata.Target metadataForFor)
        {
            var wrap = Log.Call();
		    var finalAttributes = new Dictionary<string, IAttribute>();

			// Group values by StaticName
			var valuesGroupedByStaticName = xEntity.Elements(XmlConstants.ValueNode)
				.GroupBy(v => v.Attribute(XmlConstants.KeyAttr)?.Value, e => e, (key, e) => new { StaticName = key, Values = e.ToList() });


            var envLangsSortedByPriority = _envLangs
                .OrderByDescending(p => p.Matches(_envDefLang))
                .ThenBy(p => p.EnvironmentKey)
                .ToList();

		    // Process each attribute (values grouped by StaticName)
            foreach (var sourceAttrib in valuesGroupedByStaticName)
            {
                var wrapAttrib = Log.Call($"{sourceAttrib.StaticName}");

				var xmlValuesOfAttrib = sourceAttrib.Values;
				var tempTargetValues = new List<ImportValue>();

				// Process each target's language
				foreach (var envLang in envLangsSortedByPriority)
				{
                    var maybeExactMatch = FindAttribWithLanguageMatch(envLang, xmlValuesOfAttrib);
				    var sourceValueNode = maybeExactMatch.Item1;
				    var readOnly = maybeExactMatch.Item2;

                    // Take first value if there is only one value without a dimension (default / fallback value), but only in primary language
				    if (sourceValueNode == null && xmlValuesOfAttrib.Count > 0 && envLang.Matches(_envDefLang))
				        sourceValueNode = GetFallbackAttributeInXml(xmlValuesOfAttrib);

				    // Override ReadOnly for primary target language
                    if (envLang.Matches(_envDefLang))
                        readOnly = false;

                    // Process found value
				    if (sourceValueNode != null)
				        AddNodeToImportListOrEnhancePrevious(sourceValueNode, tempTargetValues, envLang, readOnly);
                }

                // construct value elements
			    var currentAttributesImportValues = tempTargetValues.Select(tempImportValue
			            => ValueBuilder.Build(tempImportValue.XmlValue.Attribute(
			                               XmlConstants.EntityTypeAttribute)?.Value ??
			                           throw new NullReferenceException("cant' build attribute with unknown value-type"),
			                tempImportValue.XmlValue.Attribute(XmlConstants.ValueAttr)?.Value ??
			                throw new NullReferenceException("can't build attribute without value"),
			                tempImportValue.Dimensions))
			        .ToList();

                // construct the attribute with these value elements
			    var newAttr = AttributeBase.CreateTypedAttribute(sourceAttrib.StaticName, 
                    tempTargetValues.First().XmlValue.Attribute(XmlConstants.EntityTypeAttribute)?.Value,
			        currentAttributesImportValues);

                // attach to attributes-list
                finalAttributes.Add(sourceAttrib.StaticName, newAttr);
                wrapAttrib(null);
            }

		    var typeName = xEntity.Attribute(XmlConstants.AttSetStatic)?.Value;
            if(typeName == null)
                throw new NullReferenceException("trying to import an xml entity but type is null - " + xEntity);
		    
            // find out if it's a system type, and use that if it exists
            var globalType = Global.FindContentType(typeName);// as object ?? typeName;
		    var guid = Guid.Parse(xEntity.Attribute(XmlConstants.GuidNode)?.Value ??
		                          throw new NullReferenceException("can't import an entity without a guid identifier"));
		    var attribs = finalAttributes.ToDictionary(x => x.Key, y => (object) y.Value);

		    var targetEntity = globalType != null
		        ? new Entity(AppId, guid, globalType, attribs)
		        : new Entity(AppId, 0, guid, typeName, attribs);
		    if (metadataForFor != null) targetEntity.SetMetadata(metadataForFor);

            // if it's not a global type but still marked as IsJson
            // then it's a local extension type with Content-Type definitions in the app/system folder
            // in this case, the storage system must know that it should json-save it
            if (globalType == null && xEntity.Attribute(XmlConstants.EntityIsJsonAttribute)?.Value == "True")
                (targetEntity.Type as ContentType).SetSource(RepositoryTypes.Folder);

            wrap($"returning {guid} of type {globalType?.Name ?? typeName} with attribs:{attribs.Count} and metadata:{metadataForFor != null}");
			return targetEntity;
		}

        /// <summary>
        /// Either add the node to the import list with the dimensions, 
        /// OR if it is already in the queue, add the dimension information
        /// </summary>
        /// <param name="sourceValueNode"></param>
        /// <param name="tempTargetValues"></param>
        /// <param name="envLang"></param>
        /// <param name="readOnly"></param>
	    private void AddNodeToImportListOrEnhancePrevious(XElement sourceValueNode, List<ImportValue> tempTargetValues,
            TargetLanguageToSourceLanguage envLang, bool readOnly)
	    {
	        var wrapLog = Log.Call();
	        var logText = "";
	        var dimensionsToAdd = new List<ILanguage>();
	        if (_envLangs.Single(p => p.Matches(envLang.EnvironmentKey)).DimensionId > 0)
	        {
	            dimensionsToAdd.Add(new Language {Key = envLang.EnvironmentKey, ReadOnly = readOnly});
	            logText += "built dimension-list";
	        }

	        // If value has already been added to the list, add just dimension with original ReadOnly state
	        var existingImportValue = tempTargetValues.FirstOrDefault(p => p.XmlValue == sourceValueNode);
	        if (existingImportValue != null)
	        {
	            existingImportValue.Dimensions.AddRange(dimensionsToAdd);
	            logText += "targeNode already used for another node, just added dimension";
	        }
	        else
	        {
	            tempTargetValues.Add(new ImportValue
	            {
	                Dimensions = dimensionsToAdd,
	                XmlValue = sourceValueNode
	            });
	            logText += "targetNode was not used yet, added it";
	        }
	        wrapLog(logText);
	    }

	    private XElement GetFallbackAttributeInXml(List<XElement> xmlValuesOfAttrib)
	    {
	        var wrap = Log.Call();
	        // First, try to take a fallback node without language assignments
	        //var dimensionNodes = xmlValuesOfAttrib.Elements(XmlConstants.ValueDimNode);
	        var sourceValueNode = xmlValuesOfAttrib.FirstOrDefault(xv =>
	        {
	            var dimNodes = xv.Elements(XmlConstants.ValueDimNode).ToList();
	            // keep it if it has no dimensions, or if it has a dimensionId of 0
	            return !dimNodes.Any() || dimNodes.Any(x => x.Attribute(XmlConstants.DimId)?.Value == "0");
	        });

            // todo: Otherwise, try to take the primary language in file for our primary language
            // 2019-01-30 2rm: This is not needed anymore as this will be checked earlier
            // 2019-01-31 2dm - not really sure if this is true, must continue testing
            //if (sourceValueNode == null)
            //    sourceValueNode = xmlValuesOfAttrib.FirstOrDefault(xv =>
            //    {
            //        var dimNodes = xv.Elements(XmlConstants.ValueDimNode);
            //        return dimNodes.Any(d => d.Attribute(XmlConstants.DimId)?.Value == _srcDefLang);
            //    });


            // Still nothing found, just take the first one, no matter what's language it's for
            // this should probably never happen, but just in case...
            if (sourceValueNode == null)
	        {
	            Log.Warn("node still null - this indicates a problem! will just use first match");
	            sourceValueNode = xmlValuesOfAttrib.First();
	        }
	        wrap((sourceValueNode != null).ToString());
	        return sourceValueNode;
	    }

	    private Tuple<XElement, bool> FindAttribWithLanguageMatch(TargetLanguageToSourceLanguage envLang, List<XElement> xmlValuesOfAttrib)
	    {
	        var wrap = Log.Call(envLang.EnvironmentKey);
	        XElement sourceValueNode = null;
	        var readOnly = false;

            // find the xml-node which best matches the language we want to fill in
            foreach (var sourceLanguage in envLang.PriorizedDimensions)
	        {
	            var dimensionId = sourceLanguage.DimensionId.ToString();
	            // find a possible match for exactly this language
	            sourceValueNode = xmlValuesOfAttrib.FirstOrDefault(p =>
	                p.Elements(XmlConstants.ValueDimNode).Any(d => d.Attribute(XmlConstants.DimId)?.Value == dimensionId));
	            if (sourceValueNode == null) continue;

	            // if match found, check what the read/write should be
	            var textVal = sourceValueNode.Elements(XmlConstants.ValueDimNode)
	                              .FirstOrDefault(p => p.Attribute(XmlConstants.DimId)?.Value == dimensionId)?
	                              .Attribute("ReadOnly")?.Value ?? "false";

	            readOnly = bool.Parse(textVal);

	            Log.Add($"node for {envLang.EnvironmentKey} on Dim:{sourceLanguage.DimensionId}; readOnly: {readOnly}");
	            break;
	        }
	        wrap((sourceValueNode != null).ToString());
	        return new Tuple<XElement, bool>(sourceValueNode, readOnly);// sourceValueNode;
	    }


	    private class ImportValue
        {
            public XElement XmlValue;
            public List<ILanguage> Dimensions;
        }
    }
}
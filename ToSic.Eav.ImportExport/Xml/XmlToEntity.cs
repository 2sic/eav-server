using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Types;

namespace ToSic.Eav.ImportExport.Xml
{
	/// <summary>
	/// Import EAV Data from XML Format
	/// </summary>
	public class XmlToEntity
	{
        private class TargetLanguageToSourceLanguage: DimensionDefinition
        {
            //public bool Active;
            //public string EnvironmentKey;
            public List<DimensionDefinition> PriorizedDimensions = new List<DimensionDefinition>();
        }

        public int AppId { get; }
	    public XmlToEntity(int appId, List<DimensionDefinition> srcLanguages, int? srcDefLang, List<DimensionDefinition> envLanguages, string envDefLang)
	    {
	        AppId = appId;
            envLanguages = envLanguages.OrderByDescending(p => p.Matches(envDefLang)).ThenBy(p => p.EnvironmentKey).ToList();
	        _envLangs = PrepareTargetToSourceLanguageMapping(envLanguages, envDefLang, srcLanguages, srcDefLang);
	        _envDefLang = envDefLang;
            _srcDefLang = srcDefLang?.ToString();
            // prepare language mapper-lists, to later assign in case import/target have different languages
            //_relevantSrcLangsByPriority = ReduceSrcLangsToRelevantSet(srcLanguages, srcDefLang, _envLangs, _envDefLang);
	    }

	    private List<TargetLanguageToSourceLanguage> PrepareTargetToSourceLanguageMapping(List<DimensionDefinition> envLanguages, string envDefLang, List<DimensionDefinition> srcLanguages, int? srcDefLang)
	    {
            // if the environment doesn't have languages defined, we'll create a temp-entry for the main language to allow mapping
            return envLanguages.Any() 
                ? envLanguages.Select(el => new TargetLanguageToSourceLanguage {
                    Active = el.Active,
                    EnvironmentKey = el.EnvironmentKey,
                    PriorizedDimensions = FindPriorizedMatchingDimensions(el, envDefLang, srcLanguages, srcDefLang)
                }).ToList()
                : new List<TargetLanguageToSourceLanguage>
                {
                    new TargetLanguageToSourceLanguage
                    {
                        Active = true,
                        EnvironmentKey = envDefLang
	                }
                };   
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
        public Entity BuildEntityFromXml(XElement xEntity, MetadataFor metadataForFor)
		{
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
				var xmlValuesOfAttrib = sourceAttrib.Values;
				var tempTargetValues = new List<ImportValue>();

				// Process each target's language
				foreach (var envLang in envLangsSortedByPriority)
				{
					XElement sourceValueNode = null;
					var readOnly = false;
                    
                    // find the xml-node which best matches the language we want to fill in
					foreach (var sourceLanguage in envLang.PriorizedDimensions)
					{
                        var dimensionId = sourceLanguage.DimensionId.ToString();
                        // find a possible match for exactly this language
						sourceValueNode = xmlValuesOfAttrib.FirstOrDefault(p => p.Elements(XmlConstants.ValueDimNode).Any(d => d.Attribute(XmlConstants.DimId)?.Value == dimensionId));
						if (sourceValueNode == null) continue;

                        // if match found, check what the read/write should be
					    var textVal = sourceValueNode.Elements(XmlConstants.ValueDimNode)
					        .FirstOrDefault(p => p.Attribute(XmlConstants.DimId)?.Value == dimensionId)?
					        .Attribute("ReadOnly")?.Value ?? "false";

                        readOnly = bool.Parse(textVal);

						break;
					}

                    // Take first value if there is only one value without a dimension (default / fallback value), but only in primary language
                    if (sourceValueNode == null && xmlValuesOfAttrib.Count > 0 && envLang.Matches(_envDefLang))
                    {
                        // First, try to take a fallback node without language assignments
                        var dimensionNodes = xmlValuesOfAttrib.Elements(XmlConstants.ValueDimNode);
                        sourceValueNode = xmlValuesOfAttrib.FirstOrDefault(xv =>
                        {
                            var dimNodes = xv.Elements(XmlConstants.ValueDimNode);
                            return !dimensionNodes.Any() || dimensionNodes.All(x => x.Attribute(XmlConstants.DimId).Value == "0");
                        });

                        // todo: Otherwise, try to take the primary language in file for our primary language
                        // This is not needed anymore as this will be checked earlier
                        //if (sourceValueNode == null)
                        //    sourceValueNode = xmlValuesOfAttrib.FirstOrDefault(xv =>
                        //    {
                        //        var dimNodes = xv.Elements(XmlConstants.ValueDimNode);
                        //        return dimensionNodes.Any(d => d.Attribute(XmlConstants.DimId)?.Value == _srcDefLang);
                        //    });

                        // Still nothing found, just take the first one, no matter what's language it's for
                        // this should probably never happen, but just in case...
                        if (sourceValueNode == null)
                            sourceValueNode = xmlValuesOfAttrib.First();
                    }


                    // Override ReadOnly for primary target language
                    if (envLang.EnvironmentKey == _envDefLang)
                        readOnly = false;

                    // Process found value
                    if (sourceValueNode != null)
					{
						var dimensionsToAdd = new List<ILanguage>();
						if (_envLangs.Single(p => p.Matches(envLang.EnvironmentKey)).DimensionId > 0)
							dimensionsToAdd.Add(new Dimension { Key = envLang.EnvironmentKey, ReadOnly = readOnly });

						// If value has already been added to the list, add just dimension with original ReadOnly state
						var existingImportValue = tempTargetValues.FirstOrDefault(p => p.XmlValue == sourceValueNode);
						if (existingImportValue != null)
							existingImportValue.Dimensions.AddRange(dimensionsToAdd);
						else
						{
							tempTargetValues.Add(new ImportValue
							{
								Dimensions = dimensionsToAdd,
								XmlValue = sourceValueNode
							});
						}

					}

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
			}

		    var typeName = xEntity.Attribute(XmlConstants.AttSetStatic)?.Value;
            if(typeName == null)
                throw new NullReferenceException("trying to import an xml entity but type is null - " + xEntity);
		    
            // find out if it's a system type, and use that if it exists
            var contentType = Global.FindContentType(typeName);// as object ?? typeName;
		    var guid = Guid.Parse(xEntity.Attribute(XmlConstants.GuidNode)?.Value ??
		                          throw new NullReferenceException("can't import an entity without a guid identifier"));
		    var attribs = finalAttributes.ToDictionary(x => x.Key, y => (object) y.Value);

		    var targetEntity = contentType != null
		        ? new Entity(AppId, guid, contentType, attribs)
		        : new Entity(AppId, 0, guid, typeName, attribs);
		    if (metadataForFor != null) targetEntity.SetMetadata(metadataForFor);

			return targetEntity;
		}
        

	    private class ImportValue
        {
            public XElement XmlValue;
            public List<ILanguage> Dimensions;
        }
    }
}
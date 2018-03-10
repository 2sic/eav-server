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
        public int AppId { get; }
	    public XmlToEntity(int appId, List<DimensionDefinition> srcLanguages, int? srcDefLang, List<DimensionDefinition> envLanguages, string envDefLang)
	    {
	        AppId = appId;
	        _envLangs = GenerateDummyDefaultLanguageIfNecessary(envLanguages, envDefLang);
	        _envDefLang = envDefLang;
            // prepare language mapper-lists, to later assign in case import/target have different languages
            _relevantSrcLangsByPriority = ReduceSrcLangsToRelevantSet(srcLanguages, srcDefLang, _envLangs, _envDefLang);
	    }

	    private List<DimensionDefinition> GenerateDummyDefaultLanguageIfNecessary(List<DimensionDefinition> envLanguages, string defaultLanguage)
	    {
            // if the environment doesn't have languages defined, we'll create a temp-entry for the main language to allow mapping
            return envLanguages.Any() 
                ? envLanguages
                : new List<DimensionDefinition>
                {
                    new DimensionDefinition
                    {
                        Active = true,
                        EnvironmentKey = defaultLanguage,
	                    //Name = "(added by import System, default language " + defaultLanguage + ")",
	                    //Key = Constants.CultureSystemKey
	                }
                };


        }

        private readonly List<string> _relevantSrcLangsByPriority;
	    private readonly List<DimensionDefinition> _envLangs;
	    private readonly string _envDefLang;
        

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
					XElement sourceValue = null;
					var readOnly = false;

                    // find the xml-node which best matches the language we want to fill in
					foreach (var sourceLanguage in _relevantSrcLangsByPriority)
					{
                        // find a possible match for exactly this language
						sourceValue = xmlValuesOfAttrib.FirstOrDefault(p => p.Elements(XmlConstants.ValueDimNode).Any(d => d.Attribute(XmlConstants.DimId)?.Value == sourceLanguage));
						if (sourceValue == null) continue;

                        // if match found, check what the read/write should be
					    var textVal = sourceValue.Elements(XmlConstants.ValueDimNode)
					        .FirstOrDefault(p => p.Attribute(XmlConstants.DimId)?.Value == sourceLanguage)?
					        .Attribute("ReadOnly")?.Value ?? "false";

                        readOnly = bool.Parse(textVal);

						// Override ReadOnly for primary target language
						if (envLang.EnvironmentKey == _envDefLang)
							readOnly = false;

						break;
					}

					// Take first value if there is only one value wihtout a dimension (default / fallback value), but only in primary language
					if (sourceValue == null && xmlValuesOfAttrib.Count == 1 && !xmlValuesOfAttrib.Elements(XmlConstants.ValueDimNode).Any() && envLang.Matches(_envDefLang))
						sourceValue = xmlValuesOfAttrib.First();

					// Process found value
					if (sourceValue != null)
					{
						var dimensionsToAdd = new List<ILanguage>();
						if (_envLangs.Single(p => p.Matches(envLang.EnvironmentKey)).DimensionId >= 1)
							dimensionsToAdd.Add(new Dimension { Key = envLang.EnvironmentKey, ReadOnly = readOnly });

						// If value has already been added to the list, add just dimension with original ReadOnly state
						var existingImportValue = tempTargetValues.FirstOrDefault(p => p.XmlValue == sourceValue);
						if (existingImportValue != null)
							existingImportValue.Dimensions.AddRange(dimensionsToAdd);
						else
						{
							tempTargetValues.Add(new ImportValue
							{
								Dimensions = dimensionsToAdd,
								XmlValue = sourceValue
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

        /// <summary>
        /// Create a translation for one system with certain languages to another system with other languages. 
        /// Will first try exact match, then nearest match so that "de-CH" in a source will be the "de-DE" if no exact match exists.
        /// </summary>
        /// <param name="srcLangs"></param>
        /// <param name="srcDefLang"></param>
        /// <param name="envLangs"></param>
        /// <param name="envDefLang"></param>
        /// <returns></returns>
	    private static List<string> ReduceSrcLangsToRelevantSet(List<DimensionDefinition> srcLangs, int? srcDefLang,
            List<DimensionDefinition> envLangs, string envDefLang)
        {
	        var languageMap = new List<DimensionDefinition>();

            foreach (var envDim in
	            envLangs.OrderByDescending(p => p.Matches(envDefLang)).ThenBy(p => p.EnvironmentKey))
	        {
	            // Add exact match source language, if exists
	            var exactMatchSourceDimension = srcLangs.FirstOrDefault(p => envDim.Matches(p.EnvironmentKey));
	            if (exactMatchSourceDimension != null)
	                languageMap.Add(exactMatchSourceDimension);

	            // Add un-exact match language
	            var unExactMatchSourceDimensions = srcLangs.Where(
	                    sd =>
	                        !envDim.Matches(sd.EnvironmentKey) &&
	                        sd.EnvironmentKey.StartsWith(envDim.EnvironmentKey.ToLowerInvariant().Substring(0, 3)))
	                .OrderByDescending(p => p.EnvironmentKey == envDefLang)
	                .ThenByDescending(p => p.EnvironmentKey.Substring(0, 2) == p.EnvironmentKey.Substring(3, 2))
	                .ThenBy(p => p.EnvironmentKey);
	            languageMap.AddRange(unExactMatchSourceDimensions);

	            // Add primary language, if current target is primary
	            if (envDim.Matches(envDefLang) && srcDefLang.HasValue)
	            {
	                var sourcePrimaryLanguage = srcLangs.FirstOrDefault(p => p.DimensionId == srcDefLang);
	                if (sourcePrimaryLanguage != null && !languageMap.Contains(sourcePrimaryLanguage))
	                    languageMap.Add(sourcePrimaryLanguage);
	            }
	        }
	        return languageMap.Select(x => x.DimensionId.ToString()).ToList();
	    }


	    private class ImportValue
        {
            public XElement XmlValue;
            public List<ILanguage> Dimensions;
        }
    }
}
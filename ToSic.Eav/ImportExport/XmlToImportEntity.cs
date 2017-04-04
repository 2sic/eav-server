using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Import;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.ImportExport.Models;

namespace ToSic.Eav.ImportExport
{
    // todo: ensure that Dimension is pre-converted from DB.Dimension to Data.Dimension, so we can
    // extract this to import/export

	/// <summary>
	/// Import EAV Data from XML Format
	/// </summary>
	public class XmlToImportEntity
	{

		/// <summary>
		/// Returns an EAV import entity
		/// </summary>
		/// <param name="xEntity">xEntity to parse</param>
		/// <param name="assignmentObjectTypeId">assignmentObjectTypeId of the Entity</param>
		/// <param name="targetDimensions">all Dimensions that exist in the Target-App/Zone</param>
		/// <param name="sourceDimensions">all Dimensions that exist in the Source-App/Zone</param>
		/// <param name="sourceDefaultDimensionId">Default Dimension ID of the Surce-App/Zone</param>
		/// <param name="defaultLanguage">Default Language of the Target-App/Zone</param>
		/// <param name="keyNumber">KeyNumber of the Entity</param>
		/// <param name="keyGuid">KeyGuid of the Entity</param>
		/// <param name="keyString">KeyString of the Entity</param>
		public static ImpEntity BuildImpEntityFromXml(XElement xEntity, int assignmentObjectTypeId, List<Dimension> targetDimensions, List<Dimension> sourceDimensions, int? sourceDefaultDimensionId, string defaultLanguage, int? keyNumber = null, Guid? keyGuid = null, string keyString = null)
		{
			var targetEntity = new ImpEntity
			{
				AssignmentObjectTypeId = assignmentObjectTypeId,
				AttributeSetStaticName = xEntity.Attribute("AttributeSetStaticName").Value,
				EntityGuid = Guid.Parse(xEntity.Attribute("EntityGUID").Value),
				KeyNumber = keyNumber,
				KeyGuid = keyGuid,
				KeyString = keyString
			};

			var targetValues = new Dictionary<string, List<IValueImportModel>>();

			// Group values by StaticName
			var valuesGroupedByStaticName = xEntity.Elements("Value")
				.GroupBy(v => v.Attribute("Key").Value, e => e, (key, e) => new { StaticName = key, Values = e.ToList() });

			// Process each attribute (values grouped by StaticName)
			foreach (var sourceAttribute in valuesGroupedByStaticName)
			{
				var sourceValues = sourceAttribute.Values;
				var tempTargetValues = new List<ImportValue>();

				// Process each target's language
				foreach (var targetDimension in targetDimensions.OrderByDescending(p => p.ExternalKey == defaultLanguage).ThenBy(p => p.ExternalKey))
				{
					// This list will contain all source dimensions
					var sourceLanguages = new List<Dimension>();

					// Add exact match source language, if exists
					var exactMatchSourceDimension = sourceDimensions.FirstOrDefault(p => p.ExternalKey == targetDimension.ExternalKey);
					if (exactMatchSourceDimension != null)
						sourceLanguages.Add(exactMatchSourceDimension);

					// Add un-exact match language
					var unExactMatchSourceDimensions = sourceDimensions.Where(p => p.ExternalKey != targetDimension.ExternalKey && p.ExternalKey.StartsWith(targetDimension.ExternalKey.Substring(0, 3)))
						.OrderByDescending(p => p.ExternalKey == defaultLanguage)
						.ThenByDescending(p => p.ExternalKey.Substring(0, 2) == p.ExternalKey.Substring(3, 2))
						.ThenBy(p => p.ExternalKey);
					sourceLanguages.AddRange(unExactMatchSourceDimensions);

					// Add primary language, if current target is primary
                    if (targetDimension.ExternalKey == defaultLanguage && sourceDefaultDimensionId.HasValue)
					{
						var sourcePrimaryLanguage = sourceDimensions.FirstOrDefault(p => p.DimensionID == sourceDefaultDimensionId);
						if (sourcePrimaryLanguage != null && !sourceLanguages.Contains(sourcePrimaryLanguage))
							sourceLanguages.Add(sourcePrimaryLanguage);
					}

					XElement sourceValue = null;
					var readOnly = false;

					foreach (var sourceLanguage in sourceLanguages)
					{
						sourceValue = sourceValues.FirstOrDefault(p => p.Elements("Dimension").Any(d => d.Attribute("DimensionID").Value == sourceLanguage.DimensionID.ToString()));

						if (sourceValue == null)
							continue;

						readOnly = Boolean.Parse(sourceValue.Elements("Dimension").FirstOrDefault(p => p.Attribute("DimensionID").Value == sourceLanguage.DimensionID.ToString()).Attribute("ReadOnly").Value);

						// Override ReadOnly for primary target language
						if (targetDimension.ExternalKey == defaultLanguage)
							readOnly = false;

						break;
					}

					// Take first value if there is only one value wihtout a dimension (default / fallback value), but only in primary language
					if (sourceValue == null && sourceValues.Count == 1 && !sourceValues.Elements("Dimension").Any() && targetDimension.ExternalKey == defaultLanguage)
						sourceValue = sourceValues.First();

					// Process found value
					if (sourceValue != null)
					{
						var dimensionsToAdd = new List<Models.ImpDims>();
						if (targetDimensions.Single(p => p.ExternalKey == targetDimension.ExternalKey).DimensionID >= 1)
							dimensionsToAdd.Add(new Models.ImpDims { DimensionExternalKey = targetDimension.ExternalKey, ReadOnly = readOnly });

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

				var currentAttributesImportValues = tempTargetValues.Select(tempImportValue => ValueImportModel.GetModel(tempImportValue.XmlValue.Attribute("Value").Value, tempImportValue.XmlValue.Attribute("Type").Value, tempImportValue.Dimensions, targetEntity)).ToList();
				targetValues.Add(sourceAttribute.StaticName, currentAttributesImportValues);
			}

			targetEntity.Values = targetValues;

			return targetEntity;
		}


        private class ImportValue
        {
            public XElement XmlValue;
            public List<Models.ImpDims> Dimensions;
        }
    }
}
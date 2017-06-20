﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.ImportExport.Xml
{
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
		public static Entity BuildEntityFromXml(XElement xEntity, int assignmentObjectTypeId, List<Dimension> targetDimensions, List<Dimension> sourceDimensions, int? sourceDefaultDimensionId, string defaultLanguage, int? keyNumber = null, Guid? keyGuid = null, string keyString = null)
		{
		    var targetValues = new Dictionary<string, IAttribute>();// List<IValue>>();

			// Group values by StaticName
			var valuesGroupedByStaticName = xEntity.Elements(XmlConstants.ValueNode)
				.GroupBy(v => v.Attribute(XmlConstants.KeyAttr).Value, e => e, (key, e) => new { StaticName = key, Values = e.ToList() });

            // todo: prepare language mapper-lists, to later assign in case import/target have different languages
            // if(targetDimensions == null)

			// This list will contain all source dimensions
			var sourceLangs = new List<Dimension>();

		    foreach (var targetDimension in targetDimensions.OrderByDescending(p => p.Key == defaultLanguage).ThenBy(p => p.Key))
		    {
				// Add exact match source language, if exists
				var exactMatchSourceDimension = sourceDimensions.FirstOrDefault(p => p.Key == targetDimension.Key);
				if (exactMatchSourceDimension != null)
					sourceLangs.Add(exactMatchSourceDimension);

				// Add un-exact match language
				var unExactMatchSourceDimensions = sourceDimensions.Where(p => p.Key != targetDimension.Key && p.Key.StartsWith(targetDimension.Key.Substring(0, 3)))
					.OrderByDescending(p => p.Key == defaultLanguage)
					.ThenByDescending(p => p.Key.Substring(0, 2) == p.Key.Substring(3, 2))
					.ThenBy(p => p.Key);
				sourceLangs.AddRange(unExactMatchSourceDimensions);

				// Add primary language, if current target is primary
                if (targetDimension.Key == defaultLanguage && sourceDefaultDimensionId.HasValue)
				{
					var sourcePrimaryLanguage = sourceDimensions.FirstOrDefault(p => p.DimensionId == sourceDefaultDimensionId);
					if (sourcePrimaryLanguage != null && !sourceLangs.Contains(sourcePrimaryLanguage))
						sourceLangs.Add(sourcePrimaryLanguage);
				}
		    }


		    // Process each attribute (values grouped by StaticName)
            foreach (var sourceAttrib in valuesGroupedByStaticName)
			{
				var sourceValues = sourceAttrib.Values;
				var tempTargetValues = new List<ImportValue>();

				// Process each target's language
				foreach (var targetDimension in targetDimensions.OrderByDescending(p => p.Key == defaultLanguage).ThenBy(p => p.Key))
				{
					XElement sourceValue = null;
					var readOnly = false;

					foreach (var sourceLanguage in sourceLangs)
					{
						sourceValue = sourceValues.FirstOrDefault(p => p.Elements(XmlConstants.ValueDimNode).Any(d => d.Attribute(XmlConstants.DimId).Value == sourceLanguage.DimensionId.ToString()));

						if (sourceValue == null)
							continue;

						readOnly = Boolean.Parse(sourceValue.Elements(XmlConstants.ValueDimNode).FirstOrDefault(p => p.Attribute(XmlConstants.DimId).Value == sourceLanguage.DimensionId.ToString()).Attribute("ReadOnly").Value);

						// Override ReadOnly for primary target language
						if (targetDimension.Key == defaultLanguage)
							readOnly = false;

						break;
					}

					// Take first value if there is only one value wihtout a dimension (default / fallback value), but only in primary language
					if (sourceValue == null && sourceValues.Count == 1 && !sourceValues.Elements(XmlConstants.ValueDimNode).Any() && targetDimension.Key == defaultLanguage)
						sourceValue = sourceValues.First();

					// Process found value
					if (sourceValue != null)
					{
						var dimensionsToAdd = new List<ILanguage>();
						if (targetDimensions.Single(p => p.Key == targetDimension.Key).DimensionId >= 1)
							dimensionsToAdd.Add(new Dimension { Key = targetDimension.Key, ReadOnly = readOnly });

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

				var currentAttributesImportValues = tempTargetValues.Select(tempImportValue
				        => Value.Build(tempImportValue.XmlValue.Attribute(XmlConstants.EntityTypeAttribute).Value,
				            tempImportValue.XmlValue.Attribute(XmlConstants.ValueAttr).Value,
				            tempImportValue.Dimensions))
                    .ToList();
			    var newAttr = AttributeBase.CreateTypedAttribute(sourceAttrib.StaticName, tempTargetValues.First().XmlValue.Attribute(XmlConstants.EntityTypeAttribute).Value);
			    newAttr.Values = currentAttributesImportValues;

                targetValues.Add(sourceAttrib.StaticName, newAttr);
			}

            var targetEntity = new Entity(Guid.Parse(xEntity.Attribute(XmlConstants.GuidNode).Value), xEntity.Attribute(XmlConstants.AttSetStatic).Value, targetValues.ToDictionary(x => x.Key, y => (object)y.Value));
            targetEntity.SetMetadata(new Metadata
                {
                    TargetType = assignmentObjectTypeId,
                    KeyNumber = keyNumber,
                    KeyGuid = keyGuid,
                    KeyString = keyString
                });

            //targetEntity.Attributes = targetValues;

			return targetEntity;
		}


        private class ImportValue
        {
            public XElement XmlValue;
            public List<ILanguage> Dimensions;
        }
    }
}
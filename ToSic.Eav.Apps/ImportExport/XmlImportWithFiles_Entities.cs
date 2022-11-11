using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ToSic.Eav.ImportExport;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using IEntity = ToSic.Eav.Data.IEntity;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.Apps.ImportExport
{
    public partial class XmlImportWithFiles
    {
        /// <summary>
        /// Returns a collection of EAV import entities
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="assignmentObjectTypeId"></param>
        /// <returns></returns>
        private List<IEntity> BuildEntities(List<XElement> entities, int assignmentObjectTypeId)
        {
            var wrap = Log.Fn<List<IEntity>>($"for {entities?.Count}; type {assignmentObjectTypeId}");
            if(entities == null) return new List<IEntity>();
	        var result = entities.Select(e => BuildEntity(e, assignmentObjectTypeId)).ToList();
            return wrap.Return(result, $"found {result.Count}");
        }


	    /// <summary>
        /// Returns an EAV import entity
        /// </summary>
        /// <param name="entityNode">The xml-Element of the entity to import</param>
        /// <param name="targetType">assignmentObjectTypeId</param>
        /// <returns></returns>
        private IEntity BuildEntity(XElement entityNode, int targetType)
        {
            var wrap = Log.Fn<IEntity>($"assignment-type: {targetType}");

            #region retrieve optional metadata keys in the import - must happen before we apply corrections like AppId

            Guid? keyGuid = null;
		    var maybeGuid = entityNode.Attribute(XmlConstants.KeyGuid);
            if (maybeGuid != null)
                keyGuid = Guid.Parse(maybeGuid.Value);

            int? keyNumber = null;
		    var maybeNumber = entityNode.Attribute(XmlConstants.KeyNumber);
            if (maybeNumber != null)
                keyNumber = int.Parse(maybeNumber.Value);

            var keyString = entityNode.Attribute(XmlConstants.KeyString)?.Value;
            #endregion

            #region check if the xml has an own assignment object type (then we wouldn't use the default)

            // #TargetTypeIdInsteadOfTarget
            var maybeTargetTypeId = entityNode.Attribute(XmlConstants.KeyTargetType)?.Value;
            if (!string.IsNullOrWhiteSpace(maybeTargetTypeId) 
                && int.TryParse(maybeTargetTypeId, out var newTargetType)
                && newTargetType > 0)
            {
                targetType = newTargetType;
            }
            // 2022-01-19 2dm - don't use the else yet, because there are specials here like the App-case where it sets the AppId
            // or maps the cms-file. To Else-this, we would have to make sure that still happens.
            else
            {
                // Previous / Old implementation before v13
                var keyType = entityNode.Attribute(XmlConstants.KeyTargetTypeNameOld)?.Value;
                switch (keyType)
                {
                    // Special case: App AttributeSets must be assigned to the current app
                    case XmlConstants.App:
                        targetType = (int)TargetTypes.App;
                        break;
                    case XmlConstants.Entity:
                    case "Data Pipeline": // 2dm: this was an old value, 2017-08-11 this was still used in the old Employees directory app v. 1.02
                        targetType = (int)TargetTypes.Entity;
                        break;
                    case XmlConstants.ContentType:
                        targetType = (int)TargetTypes.ContentType;
                        break;
                    case XmlConstants.CmsObject:
                        // 2021-04-08 2dm warning: this line previously said `= Constants.MetadataForContentType` which seems very wrong but was never noticed
                        targetType = (int)TargetTypes.CmsItem;
                        break;
                }
            }

            // Correct any special values for specific target types
            switch (targetType)
            {
                // Special case: App Metadata must be assigned to the current app
                case (int)TargetTypes.App:
                    keyNumber = AppId;
                    break;
                case (int)TargetTypes.CmsItem:
                    if (keyString == null)
                        throw new Exception("found CmsItem but couldn't find metadata-key of type string, will abort");
                    keyString = GetMappedLink(keyString) ?? keyString;
                    break;
            }

            #endregion


            // Special case #2: Current values of Template-Describing entities, and resolve files

            foreach (var sourceValue in entityNode.Elements(XmlConstants.ValueNode))
			{
				var sourceValueString = sourceValue.Attribute(XmlConstants.ValueAttr).Value;

				// Correct FileId in Hyperlink fields (takes XML data that lists files)
			    if (!string.IsNullOrEmpty(sourceValueString) && sourceValue.Attribute(XmlConstants.EntityTypeAttribute).Value == XmlConstants.ValueTypeLink)
			    {
			        var newValue = GetMappedLink(sourceValueString);
			        if (newValue != null)
			            sourceValue.Attribute(XmlConstants.ValueAttr).SetValue(newValue);
			    }
			}

            var metadata = new Target
            {
                TargetType = targetType,
                KeyNumber = keyNumber,
                KeyGuid = keyGuid,
                KeyString = keyString
            };

            Log.A($"Metadata ({metadata.IsMetadata}) - type:{metadata.TargetType}, #:{metadata.KeyNumber} guid:{metadata.KeyGuid}, $:{metadata.KeyString}");

            var importEntity = _xmlBuilder.BuildEntityFromXml(entityNode, metadata);

            return wrap.Return(importEntity, "got it");
        }

        /// <summary>
        /// Try to map a link like "file:275" from the import to the target system
        /// Will return null if nothing appropriate found, so the caller can choose to not do anything
        /// </summary>
        /// <param name="sourceValueString"></param>
        /// <returns></returns>
	    private string GetMappedLink(string sourceValueString)
	    {
            // file
            // todo: these patterns should be stored in a global location, in case we enhance the functionality
	        var fileRegex = new Regex("^file:(?<Id>[0-9]+)", RegexOptions.IgnoreCase);
	        var a = fileRegex.Match(sourceValueString);

	        if (a.Success && a.Groups["Id"].Length > 0)
	        {
	            var originalId = int.Parse(a.Groups["Id"].Value);

	            if (_fileIdCorrectionList.ContainsKey(originalId))
	                return fileRegex.Replace(sourceValueString, "file:" + _fileIdCorrectionList[originalId]);
	        }

            // folder
            // todo: these patterns should be stored in a global location, in case we enhance the functionality
	        var folderRegEx = new Regex("^folder:(?<Id>[0-9]+)", RegexOptions.IgnoreCase);
	        var f = folderRegEx.Match(sourceValueString);

	        if (f.Success && f.Groups["Id"].Length > 0)
	        {
	            var originalId = int.Parse(f.Groups["Id"].Value);

	            if (_folderIdCorrectionList.ContainsKey(originalId))
	                return folderRegEx.Replace(sourceValueString, "folder:" + _folderIdCorrectionList[originalId]);
	        }

	        return null;
	    }

	}

}
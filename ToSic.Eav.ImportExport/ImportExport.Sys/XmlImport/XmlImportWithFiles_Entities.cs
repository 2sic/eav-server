﻿using System.Text.RegularExpressions;
using System.Xml.Linq;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.Metadata.Targets;


// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.ImportExport.Sys.XmlImport;

partial class XmlImportWithFiles
{
    /// <summary>
    /// Returns a collection of EAV import entities
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="targetTypeId"></param>
    /// <returns></returns>
    private List<IEntity> BuildEntities(List<XElement> entities, int targetTypeId) 
    {
        var l = Log.Fn<List<IEntity>>($"for {entities?.Count}; type {targetTypeId}");
        if (entities == null)
            return l.Return([], "empty");
        var result = entities
            .Select(e => BuildEntity(e, targetTypeId))
            .ToList();
        return l.Return(result, $"found {result.Count}");
    }


    /// <summary>
    /// Returns an EAV import entity
    /// </summary>
    /// <param name="entityNode">The xml-Element of the entity to import</param>
    /// <param name="targetType">targetTypeId</param>
    /// <returns></returns>
    private IEntity BuildEntity(XElement entityNode, int targetType)
    {
        var l = Log.Fn<IEntity>($"assignment-type: {targetType}");

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
                // Special case: App Content-Types must be assigned to the current app
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
                    throw new("found CmsItem but couldn't find metadata-key of type string, will abort");
                keyString = GetMappedLink(keyString) ?? keyString;
                break;
        }

        #endregion


        // Special case #2: Current values of Template-Describing entities, and resolve files

        foreach (var sourceValue in entityNode.Elements(XmlConstants.ValueNode))
        {
            var sourceValueNode = sourceValue.Attribute(XmlConstants.ValueAttr);
            if (sourceValueNode == null)
                continue; // skip if no value attribute

            var sourceValueString = sourceValueNode.Value;
            if (string.IsNullOrEmpty(sourceValueString))
                continue; // skip if no value

            // Correct FileId in Hyperlink fields (takes XML data that lists files)
            if (sourceValue.Attribute(XmlConstants.EntityTypeAttribute)?.Value == XmlConstants.ValueTypeLink)
            {
                var newValue = GetMappedLink(sourceValueString);
                if (newValue != null)
                    sourceValueNode.SetValue(newValue);
            }
        }

        var mdTarget = new Target(targetType: targetType, title: null, keyString: keyString, keyGuid: keyGuid, keyNumber: keyNumber);

        l.A($"Metadata ({mdTarget.IsMetadata}) - type:{mdTarget.TargetType}, #:{mdTarget.KeyNumber} guid:{mdTarget.KeyGuid}, $:{mdTarget.KeyString}");

        var importEntity = XmlBuilder.BuildEntityFromXml(entityNode, mdTarget);

        return l.Return(importEntity, "got it");
    }

    /// <summary>
    /// Try to map a link like "file:275" from the import to the target system
    /// Will return null if nothing appropriate found, so the caller can choose to not do anything
    /// </summary>
    /// <param name="sourceValueString"></param>
    /// <returns></returns>
    private string? GetMappedLink(string sourceValueString)
    {
        // file
        // todo: these patterns should be stored in a global location, in case we enhance the functionality
        var fileRegex = new Regex("^file:(?<Id>[0-9]+)", RegexOptions.IgnoreCase);
        var a = fileRegex.Match(sourceValueString);

        if (a.Success && a.Groups["Id"].Length > 0)
        {
            var originalId = int.Parse(a.Groups["Id"].Value);

            if (_fileIdCorrectionList.TryGetValue(originalId, out var value))
                return fileRegex.Replace(sourceValueString, "file:" + value);
        }

        // folder
        // todo: these patterns should be stored in a global location, in case we enhance the functionality
        var folderRegEx = new Regex("^folder:(?<Id>[0-9]+)", RegexOptions.IgnoreCase);
        var f = folderRegEx.Match(sourceValueString);

        if (f.Success && f.Groups["Id"].Length > 0)
        {
            var originalId = int.Parse(f.Groups["Id"].Value);

            if (_folderIdCorrectionList.TryGetValue(originalId, out var value))
                return folderRegEx.Replace(sourceValueString, "folder:" + value);
        }

        return null;
    }

}
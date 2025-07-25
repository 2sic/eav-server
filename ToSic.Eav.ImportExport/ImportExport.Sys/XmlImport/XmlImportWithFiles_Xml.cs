﻿using System.Xml.Linq;

using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.ImportExport.Sys.Xml;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.ImportExport.Sys.XmlImport;

partial class XmlImportWithFiles
{
    /// <summary>
    /// Do the import
    /// </summary>
    public bool ImportXml(int zoneId, int appId, int? parentAppId, XDocument doc, bool leaveExistingValuesUntouched = true)
    {
        var l = Log.Fn<bool>($"z#{zoneId}, a#{appId}, leaveExisting:{leaveExistingValuesUntouched}");
        // #WipDecoupleDbFromImport
        //Services.DbDataForAppImport.Value.Init(zoneId, appId, parentAppId);
            
        AppId = appId;
        ZoneId = zoneId;

        if (!IsCompatible(doc))
            return l.ReturnFalse(LogError("The import file is not compatible with the installed version of 2sxc."));

        // Get root node "SexyContent"
        var xmlSource = doc.Element(XmlConstants.RootNode);
        if (xmlSource == null)
            return l.ReturnFalse(LogError("Xml doesn't have expected root node: " + XmlConstants.RootNode));

        PrepareFolderIdCorrectionListAndCreateMissingFolders(xmlSource);
        PrepareFileIdCorrectionList(xmlSource);

        #region Prepare dimensions (languages) based on header...
        var sourceDimensions = BuildSourceDimensionsList(xmlSource);
        l.A($"build source dims list⋮{sourceDimensions?.Count}");

        var sourceDefaultLanguage = xmlSource
            .Element(XmlConstants.Header)
            ?.Element(XmlConstants.Language)
            ?.Attribute(XmlConstants.LangDefault)
            ?.Value;

        if (sourceDimensions == null || sourceDefaultLanguage == null)
            return l.ReturnFalse(LogError("Can't find source dimensions or source-default language."));

        var sourceDefaultDimensionId = sourceDimensions.Any()
            ? sourceDimensions
                .FirstOrDefault(p => p.Matches(sourceDefaultLanguage))
                ?.DimensionId
            : new();

        l.A($"source def dim:{sourceDefaultDimensionId}");

        var targetDimensions = Services.AppsCatalog.Zone(zoneId).Languages;

        XmlBuilder = Services.XmlToEntity.Value.Init(AppId, sourceDimensions, sourceDefaultDimensionId, targetDimensions, DefaultLanguage);
        #endregion

        var atsNodes = xmlSource
            .Element(XmlConstants.AttributeSets)
            ?.Elements(XmlConstants.AttributeSet)
            .ToList();
        var entNodes = xmlSource
            .Elements(XmlConstants.Entities)
            .Elements(XmlConstants.Entity)
            .ToList();

        var importTypes = GetImportContentTypes(atsNodes ?? []);
        var importEntities = BuildEntities(entNodes, (int)TargetTypes.None);


        var import = Services.ImporterLazy.Value.Init(ZoneId, AppId, leaveExistingValuesUntouched, true, parentAppId: parentAppId);

        import.ImportIntoDb(importTypes, importEntities.Cast<Entity>().ToList());

        l.A($"Purging {ZoneId}/{AppId}");
        Services.AppCachePurger.Purge(ZoneId, AppId);

        Messages.AddRange(import.Storage.ImportLogToBeRefactored);

        return l.ReturnTrue("done");
    }


}
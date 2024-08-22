﻿using System.IO;
using ToSic.Eav.Context;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Apps.Integration;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class EavImportExportEnvironmentBase: ServiceBase, IImportExportEnvironment
{
    #region constructor / DI

    /// <summary>
    /// DI Constructor
    /// </summary>
    protected EavImportExportEnvironmentBase(ISite site, IAppStates appStates, string logName) : base(logName)
    {
        Site = site;
        _appStates = appStates;
    }

    protected readonly ISite Site;
    private readonly IAppStates _appStates;

    #endregion

    public abstract List<Message> TransferFilesToSite(string sourceFolder, string destinationFolder);

    public abstract Version TenantVersion { get; }

    public string ModuleVersion => EavSystemInfo.VersionString;

    public abstract string FallbackContentTypeScope { get; }

    public string DefaultLanguage => Site.DefaultCultureCode;

    public abstract string TemplatesRoot(int zoneId, int appId);

    public abstract string GlobalTemplatesRoot(int zoneId, int appId);

    public string TargetPath(string folder)
    {
        var appPath = Path.Combine(Site.AppsRootPhysicalFull, folder);
        return appPath;
    }

    public abstract void MapExistingFilesToImportSet(Dictionary<int, string> filesAndPaths, Dictionary<int, int> fileIdMap);

    public abstract void CreateFoldersAndMapToImportIds(Dictionary<int, string> foldersAndPath, Dictionary<int, int> folderIdCorrectionList, List<Message> importLog);

    public SaveOptions SaveOptions(int zoneId) => Log.Func($"{nameof(zoneId)}:{zoneId}", () =>
    {
        var langs = _appStates.AppsCatalog.Zone(zoneId).Languages;
        var opts = new SaveOptions(DefaultLanguage, langs);
        return (opts, $"langs: {langs.Count}");
    });
}
using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Apps.Sys.FileSystemState;

[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class AppFileSystemLoaderBase(ISite siteDraft, LazySvc<IAppPathsMicroSvc> appPathsLazy, LazySvc<IZoneMapper> zoneMapper, object[]? connect = default)
    : ServiceBase(EavLogs.Eav + ".AppFSL", connect: [..connect ?? [], siteDraft, appPathsLazy, zoneMapper])
{
    #region Constants

    public const string FieldFolderPrefix = "field-";
    public const string JsFile = "index.js";

    #endregion

    public string ExtensionsPath { get; set; } = null!;
    public string ExtensionsFolder { get; set; } = null!;
    public string ExtensionsPathShared { get; set; } = null!;
    public string ExtensionsFolderShared { get; set; } = null!;

    protected IAppIdentity AppIdentity { get; private set; } = null!;
    protected ToSic.Sys.Logging.LogSettings LogSettings { get; private set; } = new();

    #region Inits

    public void Init(IAppReader appReader, ToSic.Sys.Logging.LogSettings logSettings, string? appFolderBeforeReaderIsReady = default)
    {
        LogSettings = logSettings;
        var l = Log.Fn($"{appReader.Show()}, {appReader.Specs.Folder}, ...");
        AppIdentity = appReader.PureIdentity();

        // Get the site - a bit special
        // The site to use. This should be used instead of Services.Site,
        // since in some cases (e.g. DNN Search) the initial site is not available.
        // So in that case it overrides the implementation to get the real site just-in-time.
        var site = zoneMapper.SiteOfAppIfSiteInvalid(siteDraft, AppIdentity);

        // Get the app paths helper
        var appPaths = appPathsLazy.Value.Get(appReader, site);
        // If an override is provided, use that instead of the default / automatic app folder
        if (appFolderBeforeReaderIsReady != null)
            ((AppPaths)appPaths).SetupForUseBeforeAppIsReady(appFolderBeforeReaderIsReady);
        var ok = InitPathAfterAppId(appPaths);
        l.Done(ok ? "ok" : "error");
    }

    /// <summary>
    /// Init Path After AppId must be in an own method, as each implementation may have something custom to handle this.
    /// </summary>
    /// <returns></returns>
    private bool InitPathAfterAppId(IAppPaths appPaths)
    {
        var l = Log.Fn<bool>();

        // Local app paths
        (ExtensionsPath, ExtensionsFolder) = GetBestPathAndFolder(appPaths.PhysicalPath);
        // Shared app paths
        (ExtensionsPathShared, ExtensionsFolderShared) = GetBestPathAndFolder(appPaths.PhysicalPathShared);

        return l.ReturnTrue($"p:{ExtensionsPath}, ps:{ExtensionsPathShared}");

        (string path, string folder) GetBestPathAndFolder(string appPathToCheck)
        {
            // New /extensions folder
            var extensionsNewPath = Path.Combine(appPathToCheck, FolderConstants.AppExtensionsFolder);

            // Legacy /system folder
            var extensionsLegacyPath = Path.Combine(appPathToCheck, FolderConstants.AppExtensionsLegacyFolder);

            var preferNew = Directory.Exists(extensionsNewPath) || !Directory.Exists(extensionsLegacyPath);
            return preferNew
                ? (extensionsNewPath, FolderConstants.AppExtensionsFolder)
                : (extensionsLegacyPath, FolderConstants.AppExtensionsLegacyFolder);
        }
    }

    #endregion


}
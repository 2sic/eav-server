using ToSic.Eav.Context;
using ToSic.Eav.Integration;

namespace ToSic.Eav.Apps.Integration;

[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class AppFileSystemLoaderBase(ISite siteDraft, LazySvc<IAppPathsMicroSvc> appPathsLazy, LazySvc<IZoneMapper> zoneMapper, object[] connect = default)
    : ServiceBase(EavLogs.Eav + ".AppFSL", connect: [..connect ?? [], siteDraft, appPathsLazy, zoneMapper])
{
    #region Constants

    public const string FieldFolderPrefix = "field-";
    public const string JsFile = "index.js";

    #endregion

    public string Path { get; set; }
    public string PathShared { get; set; }

    /// <summary>
    /// The site to use. This should be used instead of Services.Site,
    /// since in some cases (e.g. DNN Search) the initial site is not available.
    /// So in that case it overrides the implementation to get the real site just-in-time.
    /// </summary>
    protected ISite Site => field ??= zoneMapper.SiteOfAppIfSiteInvalid(siteDraft, AppIdentity.AppId);

    protected IAppIdentity AppIdentity { get; private set; }
    private IAppPaths _appPaths;

    #region Inits

    public AppFileSystemLoaderBase Init(IAppReader appReader, LogSettings logSettings)
    {
        LogSettings = logSettings ?? new();
        var l = Log.Fn<AppFileSystemLoaderBase>($"{appReader.AppId}, {appReader.Specs.Folder}, ...");
        AppIdentity = appReader.PureIdentity();
        _appPaths = appPathsLazy.Value.Get(appReader, Site);
        InitPathAfterAppId();
        return l.Return(this);
    }
    protected LogSettings LogSettings { get; private set; }


    /// <summary>
    /// Init Path After AppId must be in an own method, as each implementation may have something custom to handle this.
    /// </summary>
    /// <returns></returns>
    protected bool InitPathAfterAppId()
    {
        var l = Log.Fn<bool>();
        Path = System.IO.Path.Combine(_appPaths.PhysicalPath, Constants.FolderAppExtensions);
        PathShared = System.IO.Path.Combine(_appPaths.PhysicalPathShared, Constants.FolderAppExtensions);
        return l.ReturnTrue($"p:{Path}, ps:{PathShared}");
    }

    #endregion


}
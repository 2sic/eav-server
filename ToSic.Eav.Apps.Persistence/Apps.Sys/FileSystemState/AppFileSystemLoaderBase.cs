using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Apps.Sys.FileSystemState;

public record AppFileSystemLoaderOptions(IAppReader AppReader, ToSic.Sys.Logging.LogSettings LogSettings);

[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class AppFileSystemLoaderBase(ISite siteDraft, LazySvc<IAppPathsMicroSvc> appPathsLazy, LazySvc<IZoneMapper> zoneMapper, object[]? connect = default)
    : ServiceWithSetup<AppFileSystemLoaderOptions>(EavLogs.Eav + ".AppFSL", connect: [..connect ?? [], siteDraft, appPathsLazy, zoneMapper])
{
    #region Constants

    public const string FieldFolderPrefix = "field-";
    public const string JsFile = "index.js";

    #endregion
    
    private IAppPaths AppPaths => field
        ??= appPathsLazy.Value.Get(MyOptions.AppReader, zoneMapper.SiteOfAppIfSiteInvalid(siteDraft, MyOptions.AppReader.PureIdentity()));

    public string ExtensionsPath => field
        ??= Path.Combine(AppPaths.PhysicalPath, FolderConstants.AppExtensionsFolder);
    
    public string ExtensionsPathShared => field
        ??= Path.Combine(AppPaths.PhysicalPathShared, FolderConstants.AppExtensionsFolder);
    
    protected IAppIdentity AppIdentity => field
        ??= MyOptions.AppReader.PureIdentity();
    
    protected ToSic.Sys.Logging.LogSettings LogSettings => field
        ??= MyOptions.LogSettings;

}
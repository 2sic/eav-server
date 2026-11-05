using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Apps.Sys.FileSystemState;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppFileSystemLoaderPaths(ISite siteDraft, LazySvc<IAppPathsMicroSvc> appPathsLazy, LazySvc<IZoneMapper> zoneMapper)
    : ServiceWithSetup<AppFileSystemLoaderOptions>($"{EavLogs.Eav}.AFSLPaths", connect: [appPathsLazy, zoneMapper])
{
    private IAppPaths AppPaths => field
        ??= appPathsLazy.Value.Get(MyOptions.AppReader, zoneMapper.SiteOfAppIfSiteInvalid(siteDraft, MyOptions.AppReader.PureIdentity()));

    public string ExtensionsPath => field
        ??= Path.Combine(AppPaths.PhysicalPath, FolderConstants.AppExtensionsFolder);
    
    public string ExtensionsPathShared => field
        ??= Path.Combine(AppPaths.PhysicalPathShared, FolderConstants.AppExtensionsFolder);
}

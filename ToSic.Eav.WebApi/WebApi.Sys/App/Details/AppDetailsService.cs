using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Context;
using ToSic.Sys.Code.InfoSystem;

namespace ToSic.Eav.WebApi.Sys.App.Details;

/// <summary>
/// Core service to get details about an app, used by some data sources.
/// </summary>
/// <remarks>
/// This is the basic implementation, 2sxc will register another one adding lightspeed infos and thumbnails.
/// </remarks>
/// <param name="codeStats"></param>
/// <param name="appPathsGen"></param>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppDetailsService(CodeInfoStats codeStats, Generator<IAppPathsMicroSvc> appPathsGen)
    : ServiceBase("Bck.Apps", connect: [codeStats, appPathsGen]), IAppDetailsService
{
    public virtual AppDetailsRaw GetDetails(ISite site, IAppReader appReader)
    {
        var paths = appPathsGen.New().Get(appReader, site);
        var specs = appReader.Specs;
        return new()
        {
            Id = appReader.AppId,
            IsApp = specs.NameId != KnownAppsConstants.DefaultAppGuid &&
                    specs.NameId != KnownAppsConstants.PrimaryAppGuid, // #SiteApp v13
            Guid = specs.NameId,
            Name = specs.Name,
            Folder = specs.Folder,
            AppRoot = paths.Path,
            IsHidden = specs.Configuration.IsHidden,
            ConfigurationId = specs.Configuration.Id,
            Items = appReader.List.Count,
            Thumbnail = "",
            Version = specs.VersionSafe(),
            IsGlobal = appReader.IsShared(),
            IsInherited = appReader.IsInherited(),
            Lightspeed = null,
            HasCodeWarnings = codeStats.AppHasWarnings(appReader.AppId),
        };
    }
}
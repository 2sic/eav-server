using ToSic.Eav.Context;
using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Internal.Environment;

namespace ToSic.Eav.Apps.Integration;

/// <summary>
/// Find the App Paths for a specific App in a site
/// </summary>
/// <remarks>
/// IMPORTANT
/// * There is a complex problem related to Dnn which can have a module in another site.
///   This can lead to an issue where the ISite used here may be the wrong one.
/// * The reason is that the ISite is shared across modules
///   Which is difficult because it's initialized on module_load,
///   But rendering will happen later, and in between other module do the _Load which can change the site
/// * If that happens on the initial call, the cache will be wrong forever.
///   This is why as a temporary workaround we use the SiteId as part of the cache key.
///   Not sexy, but I guess better than alternatives for now.
/// * Future: We should find a way to scope some DI to a module, so it doesn't bleed to others
///   But that is a bit difficult, because there are also some services like the IPage which should be shared across modules
/// </remarks>
internal class AppPathsMicroSvc(LazySvc<IServerPaths> serverPaths, LazySvc<IGlobalConfiguration> config, LazySvc<ISite> siteLazy)
    : ServiceBase($"{EavLogs.Eav}.AppPth", connect: [serverPaths, config]), IAppPathsMicroSvc
{
    public IAppPaths Get(IAppReader appReader, ISite site)
        => new AppPaths(serverPaths, config, siteLazy, site, appReader);
}
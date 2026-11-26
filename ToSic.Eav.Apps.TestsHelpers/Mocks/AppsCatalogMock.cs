using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.Catalog;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Sys.DI;

namespace ToSic.Eav.Apps.Mocks;
public class AppsCatalogMock(IAppsCacheSelector appsCacheSelector, LazySvc<IAppLoaderTools> appLoaderTools)
    : AppsCatalog(appsCacheSelector, appLoaderTools), IAppsCatalog
{
    IAppIdentityPure IAppsCatalog.AppIdentity(int appId)
    {
        // Override the behavior if asking for the -42 App
        return appId == KnownAppsConstants.PresetIdentity.AppId
            ? KnownAppsConstants.PresetIdentity.PureIdentity()
            : base.AppIdentity(appId);
    }

    IAppIdentityPure IAppsCatalog.PrimaryAppIdentity(int zoneId)
        => KnownAppsConstants.PresetIdentity.PureIdentity();

    IAppIdentityPure IAppsCatalog.DefaultAppIdentity(int zoneId)
        => KnownAppsConstants.PresetIdentity.PureIdentity();

}

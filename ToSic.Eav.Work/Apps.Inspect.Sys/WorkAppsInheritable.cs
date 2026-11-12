using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Context;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Inspect.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkAppsInheritable(IAppStateCacheService appStates, IAppReaderFactory appReaders, IAppsCatalog appsCatalog)
    : ServiceBase("Cms.AppsRt", connect: [appStates, appReaders, appsCatalog])
{

    /// <summary>
    /// Returns all Apps for the current zone
    /// </summary>
    /// <returns></returns>
    public ICollection<IAppReader> GetInheritableApps(ISite site)
    {
        var l = Log.Fn<ICollection<IAppReader>>();

        var defaultAppId = appsCatalog.DefaultAppIdentity(site.ZoneId).AppId;

        // Get existing apps, as we should not list inheritable apps which are already inherited
        var siteApps = appsCatalog.Apps(site.ZoneId)
            .Select(a => appReaders.Get(new AppIdentityPure(site.ZoneId, a.Key)))
            // Content is registered by default; only block inheritance once it has real data.
            .Where(appReader => appReader.AppId != defaultAppId || appReader.List.Count > 3)
            .Select(a => a.Specs.Folder)
            .ToListOpt();

        var zones = appsCatalog.Zones;
        var result = zones
            // Skip all global apps on the current site, as they shouldn't be inheritable in this site
            .Where(z => z.Key != site.ZoneId)
            .SelectMany(zSet =>
            {
                var zId = zSet.Key;
                var appIds = appsCatalog.Apps(zId);

                return appIds
                    .Select(a => new AppIdentityPure(zId, a.Key))
                    // Skip all which are not yet in memory - not perfect
                    // ...but otherwise it can take very long after a restart, especially with hundreds of sites
                    // Means that for us to be able to find a master app
                    // that apps-management must have been accessed at least once after the restart, so that the app is in memory
                    .Where(appStates.IsCached)
                    .Select(appReaders.Get)
                    .Where(reader =>
                        // Only show apps which are shared
                        reader.IsShared()
                        // and not already in the current site (with the same folder name)
                        && !siteApps.Any(sa => sa.EqualsInsensitive(reader.Specs.Folder)))
                    .OrderBy(reader => reader.Specs.Name)
                    .ToListOpt();
            })
            .ToListOpt();

        return l.Return(result);
    }

}
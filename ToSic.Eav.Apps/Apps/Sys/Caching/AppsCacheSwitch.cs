using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Sys.Caching;

/// <summary>
/// This will retrieve the current AppCache.
/// It selects the best enabled keyed cache, falling back to the default cache.
/// </summary>
/// <remarks>
/// This class is transient and resolves singleton cache implementations.
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppsCacheSwitch(
    IServiceProvider serviceProvider,
    ISysFeaturesService featuresService,
    LazySvc<IAppLoaderTools> appLoaderTools
    )
    : ServiceBase($"{LogScopes.Lib}.AppCch"), IAppCachePurger
{
    public IAppLoaderTools AppLoaderTools => appLoaderTools.Value;

    public IAppsCache Value => field ??= GetOnceDuringCurrentRequest();

    public void Purge(IAppIdentity app)
        => Value.Purge(app);


    private IAppsCache GetOnceDuringCurrentRequest()
        => TryBuildIfEnabled(BuiltInFeatures.WebFarmCacheDebug, nameof(BuiltInFeatures.WebFarmCacheDebug))
           ?? TryBuildIfEnabled(BuiltInFeatures.WebFarmCache, nameof(BuiltInFeatures.WebFarmCache))
           ?? serviceProvider.Build<IAppsCache>(AppsCache.DefaultNameId);

    private IAppsCache? TryBuildIfEnabled(Feature feature, string key)
        => featuresService.IsEnabled(feature)
            ? serviceProvider.TryBuild<IAppsCache>(key)
            : null;

    public void Update(IAppIdentity app, IEnumerable<int> entities)
        => Value.Update(app, entities, Log, appLoaderTools.Value);

}

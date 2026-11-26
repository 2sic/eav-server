using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Sys.Caching;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Sys.Caching;

/// <summary>
/// This will retrieve the current AppCache.
/// Basically it will get all registered <see cref="IAppsCache"/> and get the first which says it's valid.
/// Once it's found the right one, it will keep returning that,
/// unless the features report that a setting has been changed, in which case it will re-evaluate the best match. 
/// </summary>
/// <remarks>
/// This class is transient, but it uses a static cache for the value, so the returned value behaves as a "safe" singleton.
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppsCacheSwitch(
    ILogStore logStore,
    ISysFeaturesService featuresService,
    Generator<IAppsCache> appsCache,
    LazySvc<IAppLoaderTools> appLoaderTools)
    : ServiceBase("Eav.ApChSw", connect: [featuresService, appLoaderTools]),
        ICacheDependent,
        IAppCachePurger
{
    public IAppLoaderTools AppLoaderTools => appLoaderTools.Value;

    #region Main Value Handling (access static presolved value, optionally reset)

    public IAppsCache Value => field ??= GetOnceDuringCurrentRequest();

    public void Purge(IAppIdentity app)
        => Value.Purge(app);

    /// <summary>
    /// TODO: change this to use the key as provided by the featuresService
    /// </summary>
    /// <returns></returns>
    private string GetPreferredKey()
    {
        return AppsCache.DefaultNameId;
    }

    private IAppsCache GetOnceDuringCurrentRequest()
    {
        // First call and/or not created yet, retrieve as expected
        if (!CacheChanged())
            return appsCache.New(GetPreferredKey());

        // If the cache has changed, we will

        // Otherwise reset the previously Singleton result and force new retrieve
        logStore.Add(LogNames.LogStoreServiceSwitcher, Log);
        Log.Fn(message: "Cache changed; re-check singleton service");

        // Now update the timestamp, so next iteration will know if the cache has changed again
        CacheTimestamp = featuresService.CacheTimestamp;
        return appsCache.New(GetPreferredKey());
    }

    public long CacheTimestamp { get; private set; }

    /// <summary>
    /// Regard the cache as having changed, if the feature service changes.
    /// This is important, since features can change which cache is to be used. 
    /// </summary>
    /// <returns></returns>
    public bool CacheChanged() => featuresService.CacheChanged(CacheTimestamp);

    #endregion

    public void Update(IAppIdentity app, IEnumerable<int> entities)
        => Value.Update(app, entities, Log, appLoaderTools.Value);

}
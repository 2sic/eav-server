using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Internal.Features;
using ToSic.Lib.DI;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Caching;

/// <summary>
/// This will retrieve the current AppCache.
/// Basically it will get all registered <see cref="IAppsCacheSwitchable"/> and get the first which says it's valid.
/// Once it's found the right one, it will keep returning that,
/// unless the features report that a setting has been changed, in which case it will re-evaluate the best match. 
/// </summary>
/// <remarks>
/// This class is transient, but it uses a static cache for the value, so the returned value behaves as a "safe" singleton.
/// </remarks>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppsCacheSwitch(
    ILogStore logStore,
    IEavFeaturesService featuresService,
    LazySvc<ServiceSwitcher<IAppsCacheSwitchable>> serviceSwitcher,
    LazySvc<IAppLoaderTools> appLoaderTools
    )
    : ServiceSwitcherSingleton<IAppsCacheSwitchable>(logStore, serviceSwitcher, connect: [featuresService, appLoaderTools]),
        ICacheDependent
{
    public IAppLoaderTools AppLoaderTools => appLoaderTools.Value;

    #region Main Value Handling (access static presolved value, optionally reset)

    public new IAppsCacheSwitchable Value => _value.Get(GetOnceDuringCurrentRequest);
    private readonly GetOnce<IAppsCacheSwitchable> _value = new();

    private IAppsCacheSwitchable GetOnceDuringCurrentRequest()
    {
        // First call and/or not created yet, retrieve as expected
        if (!CacheChanged()) return base.Value;

        // Otherwise reset the previously Singleton result and force new retrieve
        Reset();
        _cacheTimestamp = featuresService.CacheTimestamp;
        return base.Value;
    }

    public long CacheTimestamp => _cacheTimestamp;
    private static long _cacheTimestamp;

    /// <summary>
    /// Regard the cache as having changed, if the feature service changes.
    /// This is important, since features can change which cache is to be used. 
    /// </summary>
    /// <returns></returns>
    public bool CacheChanged() => featuresService.CacheChanged(CacheTimestamp);

    #endregion

    public IAppStateCache Update(IAppIdentity app, IEnumerable<int> entities)
        => Value.Update(app, entities, Log, appLoaderTools.Value);

}
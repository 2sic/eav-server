using ToSic.Eav.Internal.Features;
using ToSic.Lib.DI;
using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Caching;

/// <summary>
/// This will retrieve the current AppCache.
/// Basically it will get all registered <see cref="IAppsCacheSwitchable"/> and get the first which says it's valid.
/// Once it's found the right one, it will keep returning that,
/// unless the features report that a setting has been changed, in which case it will re-evaluate the best match. 
/// </summary>
public class AppsCacheSwitch : ServiceSwitcherSingleton<IAppsCacheSwitchable>, ICacheDependent
{
    public AppsCacheSwitch(
        ILogStore logStore,
        IEavFeaturesService featuresService,
        LazySvc<ServiceSwitcher<IAppsCacheSwitchable>> serviceSwitcher
    ) : base(logStore, serviceSwitcher) =>
        ConnectServices(_featuresService = featuresService);

    private readonly IEavFeaturesService _featuresService;

    public new IAppsCacheSwitchable Value => _value.Get(GetOnceDuringCurrentRequest);
    private readonly GetOnce<IAppsCacheSwitchable> _value = new();

    private IAppsCacheSwitchable GetOnceDuringCurrentRequest()
    {
        // First call and/or not created yet, retrieve as expected
        if (!CacheChanged()) return base.Value;

        // Otherwise reset the previously Singleton result and force new retrieve
        Reset();
        _cacheTimestamp = _featuresService.CacheTimestamp;
        return base.Value;
    }


    public long CacheTimestamp => _cacheTimestamp;
    private static long _cacheTimestamp;

    public bool CacheChanged() => _featuresService.CacheChanged(CacheTimestamp);
}
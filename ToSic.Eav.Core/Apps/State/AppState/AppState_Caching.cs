using ToSic.Eav.Caching;
using ToSic.Eav.Data.PiggyBack;

namespace ToSic.Eav.Apps.State;

partial class AppState: IAppStateCache, ICacheExpiring, IHasPiggyBack, ICanBeCacheDependency
{
    /// <summary>
    /// Helper object to keep track of cache changes
    /// </summary>
    [PrivateApi]
    ICacheStatistics IAppStateCache.CacheStatistics { get; } = new CacheStatistics();

    /// <inheritdoc />
    public long CacheTimestamp => CacheTimestampDelegate.CacheTimestamp;

    private void CacheResetTimestamp(string message, int offset = 0)
    {
        // Update time stamp
        // In very rare, fast cases the timestamp is unmodified
        // In such cases we must make sure it's incremented by at least 1
        var prevTimeStamp = CacheTimestamp;
        CacheTimestampPrivate.CacheTimestamp = DateTime.Now.Ticks + offset;
        if (prevTimeStamp == CacheTimestampPrivate.CacheTimestamp)
            CacheTimestampPrivate.CacheTimestamp++;

        (this as IAppStateCache).CacheStatistics.Update(CacheTimestamp, Index.Count, message);
        Log.A($"cache reset to stamp {CacheTimestamp} = {CacheTimestamp.ToReadable()}");
        Log.A($"Stats: ItemCount: {Index.Count}; ResetCount: {(this as IAppStateCache).CacheStatistics.ResetCount}  Message: '{message}'");

        // publish event so lightspeed can flush cache
        MemoryCacheService.Notify(this);
    }

    /// <summary>
    /// Call this method before AppState object is destroyed and recreate (new object will get new reference)
    /// to ensure that dependent object are notified.  
    /// </summary>
    // IMPORTANT: This is called by the farm cache, which is not part of this solution. That's why you don't have any access-counts
    [PrivateApi]
    void IAppStateCache.PreRemove() => CacheResetTimestamp("AppState object will be destroyed and recreated as new object", 1);


    /// <inheritdoc />
    public bool CacheChanged(long dependentTimeStamp) => CacheTimestamp > dependentTimeStamp;

    /// <summary>
    /// The App can itself be the master of expiry, or it can be that a parent-app must be included
    /// So the expiry-provider is this object, which must be initialized on AppState creation
    /// </summary>
    private ICacheExpiring CacheTimestampDelegate { get; }

    /// <summary>
    /// Store for the app-private timestamp. In inherited apps, it will be combined with the parent using the CacheExpiryDelegate
    /// </summary>
    private CacheExpiring CacheTimestampPrivate { get; } = new();

    /// <summary>
    /// Create an expiry source for this app.
    /// In normal mode it will only use the private timestamp.
    /// In shared mode it will merge its timestamp with the parent
    /// </summary>
    private static ICacheExpiring CreateExpiryDelegate(ParentAppState? parent, ICacheExpiring cacheTimestampPrivate)
        => (parent.InheritContentTypes || parent.InheritEntities) && parent.AppState != null
            ? new CacheExpiringMultiSource(cacheTimestampPrivate, parent.AppState)
            : cacheTimestampPrivate;

    [PrivateApi] 
    public PiggyBack PiggyBack => field ??= new();

    #region CacheDependency

    public bool CacheIsNotifyOnly => true;

    string ICanBeCacheDependency.CacheDependencyId => $"{typeof(AppState).FullName}({this.Show()})";

    #endregion
}
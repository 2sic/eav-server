namespace ToSic.Sys.Caching.Synchronized;

/// <summary>
/// Object which can be used to cache a single object, which is dependent on an upstream cache.
/// </summary>
/// <typeparam name="T">The type which is enumerated, usually an `IEntity`</typeparam>
/// <param name="upstream">the upstream cache which can tell us if a refresh is necessary</param>
/// <param name="rebuild">the method which rebuilds the list</param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class SynchronizedObject<T>(ICacheExpiring upstream, Func<T> rebuild) : ICacheDependent, ICacheExpiring
{
    /// <summary>
    /// Upstream source which implements <see cref="ICacheExpiring"/> to tell this object when the data must be refreshed
    /// </summary>
    protected readonly ICacheExpiring Upstream = upstream;

    /// <summary>
    /// The cached object/result
    /// </summary>
    protected T? Cache;
        
    /// <summary>
    /// A callback to rebuild the cache which is provided when this object is created 
    /// </summary>
    protected readonly Func<T> RebuildCache = rebuild;
        
    /// <summary>
    /// Counter to see how often the cache had been reset.
    /// </summary>
    public int RebuildCount;


    /// <summary>
    /// Retrieves the list - either the cache one, or if timestamp has changed, rebuild and return that
    /// </summary>
    public virtual T Value
    {
        get
        {
            if (Cache != null && !CacheChanged())
                return Cache;

            // First sync time stamp, just in case it will change during rebuild
            CacheTimestamp = Upstream.CacheTimestamp;

            // Rebuild the cache
            Cache = RebuildCache();

            // Update statistics
            RebuildCount++;

            // Done
            return Cache;
        }
    }

    /// <inheritdoc />
    public long CacheTimestamp { get; private set; }

    /// <inheritdoc />
    public bool CacheChanged(long dependentTimeStamp)
        => Upstream.CacheChanged(dependentTimeStamp);

    /// <inheritdoc />
    public bool CacheChanged()
        => Upstream.CacheChanged(CacheTimestamp);
}
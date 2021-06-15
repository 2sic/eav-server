using System;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Caching
{
    /// <summary>
    /// WIP 12.03
    /// </summary>
    /// <typeparam name="T">The type which is enumerated, usually an <see cref="IEntity"/></typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public class SynchronizedObject<T>: ICacheDependent, ICacheExpiring
    {
        /// <summary>
        /// Upstream source which implements <see cref="ICacheExpiring"/> to tell this object when the data must be refreshed
        /// </summary>
        protected readonly ICacheExpiring Upstream;

        protected T Cache;
        protected readonly Func<T> RebuildCache;


        /// <summary>
        /// Initialized a new list which depends on another source
        /// </summary>
        /// <param name="upstream">the upstream cache which can tell us if a refresh is necessary</param>
        /// <param name="rebuild">the method which rebuilds the list</param>
        public SynchronizedObject(ICacheExpiring upstream, Func<T> rebuild)
        {
            Upstream = upstream;
            RebuildCache = rebuild;
        }


        /// <summary>
        /// Retrieves the list - either the cache one, or if timestamp has changed, rebuild and return that
        /// </summary>
        public virtual T Value
        {
            get
            {
                if (Cache != null && !CacheChanged()) return Cache;

                Cache = RebuildCache();
                CacheTimestamp = Upstream.CacheTimestamp;
                return Cache;
            }
        }

        /// <inheritdoc />
        public long CacheTimestamp { get; private set; }

        /// <inheritdoc />
        public bool CacheChanged(long newCacheTimeStamp) => Upstream.CacheChanged(newCacheTimeStamp);

        /// <inheritdoc />
        public bool CacheChanged() => Upstream.CacheChanged(CacheTimestamp);
    }
}

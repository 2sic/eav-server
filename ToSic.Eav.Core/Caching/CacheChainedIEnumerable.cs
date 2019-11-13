using System;
using System.Collections;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Caching
{
    /// <summary>
    /// This is an IEnumerable which relies on an up-stream cache, which may change. That would require this IEnumerable to update what it delivers.
    /// </summary>
    /// <typeparam name="T">The type which is enumerated, usually an <see cref="IEntity"/></typeparam>
    [PublicApi]
    public class CacheChainedIEnumerable<T>: IEnumerable<T>, ICacheDependent, ICacheExpiring
    {
        /// <summary>
        /// Upstream source which implements <see cref="ICacheExpiring"/> to tell this object when the data must be refreshed
        /// </summary>
        protected readonly ICacheExpiring Upstream;

        private List<T> _cache;
        private readonly Func<List<T>> _rebuild;


        /// <summary>
        /// Initialized a new list which depends on another source
        /// </summary>
        /// <param name="upstream">the upstream cache which can tell us if a refresh is necessary</param>
        /// <param name="rebuild">the method which rebuilds the list</param>
        public CacheChainedIEnumerable(ICacheExpiring upstream, Func<List<T>> rebuild)
        {
            Upstream = upstream;
            _rebuild = rebuild;
        }


        private List<T> GetCache()
        {
             if (_cache != null && !CacheChanged()) return _cache;

            _cache = _rebuild.Invoke();
            CacheTimestamp = Upstream.CacheTimestamp;

            return _cache;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() 
            => GetCache().GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public long CacheTimestamp { get; private set; }

        /// <inheritdoc />
        public bool CacheChanged(long newCacheTimeStamp) => Upstream.CacheChanged(newCacheTimeStamp);

        /// <inheritdoc />
        public bool CacheChanged() => Upstream.CacheChanged(CacheTimestamp);
    }
}

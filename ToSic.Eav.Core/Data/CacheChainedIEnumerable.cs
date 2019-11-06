using System;
using System.Collections;
using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    public class CacheChainedIEnumerable<T>: IEnumerable<T>, ICacheDependent, ICacheExpiring
    {
        protected readonly ICacheExpiring Upstream;
        private List<T> _cache;
        private readonly Func<List<T>> _rebuild;


        /// <summary>
        /// Initialized a new list which depends on another source
        /// </summary>
        /// <param name="upstream">the upstream cache which can tell us if a refresh is necessary</param>
        /// <param name="rebuild">the method wich rebuilds the list</param>
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

        public IEnumerator<T> GetEnumerator() 
            => GetCache().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public long CacheTimestamp { get; private set; }
        public bool CacheChanged(long newCacheTimeStamp) => Upstream.CacheChanged(newCacheTimeStamp);

        public bool CacheChanged() => Upstream.CacheChanged(CacheTimestamp);
    }
}

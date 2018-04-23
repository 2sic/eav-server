using System;
using System.Collections;
using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
    public class UpstreamDependentIEnumerable<T>: IEnumerable<T>, ICacheDependent, ICacheExpiring
    {
        protected readonly ICacheExpiring Upstream;
        private List<T> _cache;
        private readonly Func<List<T>> _rebuild;

        public UpstreamDependentIEnumerable(ICacheExpiring upstream, Func<List<T>> rebuild)
        {
            Upstream = upstream;
            _rebuild = rebuild;
        }


        public List<T> GetCache()
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
        public bool CacheChanged(long prevCacheTimestamp) => Upstream.CacheChanged(prevCacheTimestamp);

        public bool CacheChanged() => Upstream.CacheChanged(CacheTimestamp);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Caching
{
    /// <summary>
    /// This is an IEnumerable which relies on an up-stream cache, which may change. That would require this IEnumerable to update what it delivers.
    /// </summary>
    /// <typeparam name="T">The type which is enumerated, usually an <see cref="IEntity"/></typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public class SynchronizedList<T>: IEnumerable<T>, ICacheDependent, ICacheExpiring
    {
        /// <summary>
        /// Upstream source which implements <see cref="ICacheExpiring"/> to tell this object when the data must be refreshed
        /// </summary>
        protected readonly ICacheExpiring Upstream;

        private IImmutableList<T> _immutableCache;
        private readonly Func<IImmutableList<T>> _rebuildImmutable;

        /// <summary>
        /// Initialized a new list which depends on another source
        /// </summary>
        /// <param name="upstream">the upstream cache which can tell us if a refresh is necessary</param>
        /// <param name="rebuild">the method which rebuilds the list</param>
        [Obsolete("You should prefer the Func<Immutable> signature")]
        public SynchronizedList(ICacheExpiring upstream, Func<List<T>> rebuild)
        {
            Upstream = upstream;
            _rebuildImmutable = () => rebuild().ToImmutableArray();
        }
        /// <summary>
        /// Initialized a new list which depends on another source
        /// </summary>
        /// <param name="upstream">the upstream cache which can tell us if a refresh is necessary</param>
        /// <param name="rebuild">the method which rebuilds the list</param>
        public SynchronizedList(ICacheExpiring upstream, Func<IImmutableList<T>> rebuild)
        {
            Upstream = upstream;
            _rebuildImmutable = rebuild;
        }


        /// <summary>
        /// Retrieves the list - either the cache one, or if timestamp has changed, rebuild and return that
        /// </summary>
        [PrivateApi("Experimental, trying to lower memory footprint")]
        public virtual IImmutableList<T> List
        {
            get
            {
                if (_immutableCache != null && !CacheChanged()) return _immutableCache;

                _immutableCache = _rebuildImmutable();
                CacheTimestamp = Upstream.CacheTimestamp;
                return _immutableCache;
            }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => List.GetEnumerator();

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

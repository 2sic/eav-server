using System.Collections.Generic;
using ToSic.Eav.Caching;

namespace ToSic.Eav.Data.LazySource
{
    /// <summary>
    /// A fake lazy entities, which just delivers a list of items without itself ever expiring.
    /// </summary>
    public class UnLazyEntities : IEntitiesSource, ICacheExpiring, ICacheDependent
    {
        public IList<IEntity> Source { get; }
        public UnLazyEntities(IList<IEntity> source)
        {
            Source = source;
        }

        public long CacheTimestamp { get; }
        public bool CacheChanged() => false;

        public bool CacheChanged(long newCacheTimeStamp) => false;

        public IEnumerable<IEntity> List => Source;
    }
}

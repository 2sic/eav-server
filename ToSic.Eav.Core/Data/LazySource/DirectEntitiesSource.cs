using System.Collections.Generic;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// An entities source which directly delivers the given entities
    /// </summary>
    public class DirectEntitiesSource : IEntitiesSource
    {
        public DirectEntitiesSource(List<IEntity> entities) => List = entities;
        public IEnumerable<IEntity> List { get; }

        public long CacheTimestamp => 0;

        /// <summary>
        /// Return false for cache changed to prevent reloading the cache unnecessarily
        /// </summary>
        public bool CacheChanged(long dependentTimeStamp) => false;

    }
}
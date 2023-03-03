using System;
using System.Collections.Generic;

namespace ToSic.Eav.Data
{
    // TODO: 2dm 2021-04-07 - must check if we still need this, I think the EAV has a similar functionality built-in
    
    /// <summary>
    /// An entities source which directly delivers the given entities
    /// </summary>
    public class DirectEntitiesSource : IEntitiesSource
    {
        public DirectEntitiesSource(IEnumerable<IEntity> entities)
        {
            CacheTimestamp = Int64.MinValue;
            List = entities;
        }

        public long CacheTimestamp { get; }

        /// <summary>
        /// The SharePoint Datasource can't figure out easily whether items have been changed
        /// Return false for cache changed to prevent reloading the cache unnecessarily
        /// </summary>
        /// <param name="dependentTimeStamp"></param>
        /// <returns></returns>
        public bool CacheChanged(long dependentTimeStamp)
        {
            return false;
        }

        public IEnumerable<IEntity> List { get; }
    }
}
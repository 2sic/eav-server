using System;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Caching
{
    public class SynchronizedEntityList: SynchronizedList<IEntity>
    {
        public SynchronizedEntityList(ICacheExpiring upstream, Func<IImmutableList<IEntity>> rebuild) 
            : base(upstream, rebuild) { }

        /// <summary>
        /// Retrieves the list - either the cache one, or if timestamp has changed, rebuild and return that
        /// </summary>
        [PrivateApi("Experimental")]
        public override IImmutableList<IEntity> List
        {
            get
            {
                if (_entityList != null && !CacheChanged()) return _entityList;
                _entityList = new ImmutableSmartList(base.List);
                return _entityList;
            }
        }

        private ImmutableSmartList _entityList;

    }
}

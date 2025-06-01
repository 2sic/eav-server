using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Lib.Caching;
using ToSic.Lib.Caching.Synchronized;

namespace ToSic.Eav.Caching;

/// <summary>
/// Specialized form of SynchronizedList which only offers entities, but these
/// in a signature that have ultra-fast lookups. 
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class SynchronizedEntityList(ICacheExpiring upstream, Func<IImmutableList<IEntity>> rebuild)
    : SynchronizedList<IEntity>(upstream, rebuild)
{
    /// <summary>
    /// Retrieves the list - either the cache one, or if timestamp has changed, rebuild and return that
    /// </summary>
    [PrivateApi("Experimental")]
    public override IImmutableList<IEntity> List
    {
        get
        {
            if (_entityList != null && !CacheChanged()) return _entityList;
            _entityList = ImmutableSmartList.Wrap(base.List);
            return _entityList;
        }
    }

    private ImmutableSmartList _entityList;

}
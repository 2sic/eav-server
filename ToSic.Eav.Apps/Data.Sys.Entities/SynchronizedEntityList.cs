﻿using System.Collections.Immutable;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Sys.Caching;
using ToSic.Sys.Caching.Synchronized;

namespace ToSic.Eav.Data.Entities.Sys;

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
        => _entityList != null && !CacheChanged()
            ? _entityList
            : _entityList = ImmutableSmartList.Wrap(base.List);

    private ImmutableSmartList? _entityList;

}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ToSic.Eav.Data.Source;

/// <summary>
/// An entities source which directly delivers the given entities
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class DirectEntitiesSource : IEntitiesSource
{
    public static TResult Using<TResult>(Func<(DirectEntitiesSource Source, List<IEntity> List), TResult> action)
    {
        var list = new List<IEntity>();
        var created = new DirectEntitiesSource(list);
        var result = action((created, list));
        created.List = list.ToImmutableList();
        created.CacheTimestamp = DateTime.Now.Ticks + 1; // just in case it's so fast that we would still get the same tick
        return result;
    }


    protected DirectEntitiesSource(IEnumerable<IEntity> entities) => List = entities;
    public IEnumerable<IEntity> List { get; private set; }

    public long CacheTimestamp { get; private set; } = DateTime.Now.Ticks;

    /// <summary>
    /// Return false for cache changed to prevent reloading the cache unnecessarily
    /// </summary>
    public bool CacheChanged(long dependentTimeStamp) => CacheTimestamp > dependentTimeStamp;

}
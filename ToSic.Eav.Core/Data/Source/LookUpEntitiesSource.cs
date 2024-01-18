using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ToSic.Eav.Data.Source;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LookUpEntitiesSource<TKey>(IEnumerable<TKey> keys, ILookup<TKey, IEntity> lookup)
    : IEntitiesSource, IEnumerable<IEntity>
{
    public IImmutableList<TKey> Keys { get; } = keys?.ToImmutableList() ?? throw new ArgumentNullException(nameof(keys));
    public ILookup<TKey, IEntity> Lookup { get; } = lookup ?? throw new ArgumentNullException(nameof(lookup));

    public long CacheTimestamp { get; } = DateTime.Now.Ticks;
    public bool CacheChanged(long dependentTimeStamp) => false; // TODO: MAY NEED TO CHANGE

    public IEnumerable<IEntity> List => Keys
        .SelectMany(lookupId => Lookup[lookupId])
        .ToList();

    public IEnumerator<IEntity> GetEnumerator() => List.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
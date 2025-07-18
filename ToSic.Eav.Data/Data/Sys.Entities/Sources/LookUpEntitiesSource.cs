﻿using System.Collections;
using System.Collections.Immutable;

namespace ToSic.Eav.Data.Sys.Entities.Sources;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class LookUpEntitiesSource<TKey>(IEnumerable<TKey> keys, ILookup<TKey, IEntity> lookup)
    : IEntitiesSource, IEnumerable<IEntity>
{
    public IImmutableList<TKey> Keys { get; } = keys?.ToImmutableOpt()
                                                ?? throw new ArgumentNullException(nameof(keys));
    public ILookup<TKey, IEntity> Lookup { get; } = lookup ?? throw new ArgumentNullException(nameof(lookup));

    public long CacheTimestamp { get; } = DateTime.Now.Ticks;
    public bool CacheChanged(long dependentTimeStamp) => false; // TODO: MAY NEED TO CHANGE

    [field: AllowNull, MaybeNull]
    public IEnumerable<IEntity> List => field
        ??= Keys
            .SelectMany(lookupId => Lookup[lookupId])
            .ToListOpt();

    public IEnumerator<IEntity> GetEnumerator() => List.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
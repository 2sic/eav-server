﻿using System.Collections;
using System.Collections.Immutable;

namespace ToSic.Sys.Caching.Synchronized;

/// <summary>
/// This is an IEnumerable which relies on an up-stream cache, which may change. That would require this IEnumerable to update what it delivers.
/// </summary>
/// <typeparam name="T">The type which is enumerated, usually an `IEntity`</typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class SynchronizedList<T>: SynchronizedObject<IImmutableList<T>>, IEnumerable<T>
{
    /// <summary>
    /// Initialized a new list which depends on another source
    /// </summary>
    /// <param name="upstream">the upstream cache which can tell us if a refresh is necessary</param>
    /// <param name="rebuild">the method which rebuilds the list</param>
    [Obsolete("You should prefer the Func<Immutable> signature")]
    public SynchronizedList(ICacheExpiring upstream, Func<List<T>> rebuild): base(upstream, () => rebuild().ToImmutableOpt())
    {
    }

    /// <summary>
    /// Initialized a new list which depends on another source
    /// </summary>
    /// <param name="upstream">the upstream cache which can tell us if a refresh is necessary</param>
    /// <param name="rebuild">the method which rebuilds the list</param>
    public SynchronizedList(ICacheExpiring upstream, Func<IImmutableList<T>> rebuild): base(upstream, rebuild)
    {
    }


    /// <summary>
    /// Retrieves the list - either the cache one, or if timestamp has changed, rebuild and return that
    /// </summary>
    [PrivateApi("Experimental, trying to lower memory footprint")]
    public virtual IImmutableList<T> List => Value;

    [PrivateApi]
    public IEnumerator<T> GetEnumerator() => List.GetEnumerator();

    [PrivateApi]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
}
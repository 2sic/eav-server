﻿using ToSic.Eav.Data;

namespace ToSic.Eav.Caching;

/// <summary>
/// Object container with additional timestamp
/// </summary>
/// <typeparam name="T">The type which is enumerated, usually an <see cref="IEntity"/></typeparam>
[PrivateApi("this is just fyi")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Timestamped<T>(T value, long timestamp) : ITimestamped
{
    /// <summary>
    /// The cached object/result
    /// </summary>
    public T Value { get; } = value;


    /// <inheritdoc />
    public long CacheTimestamp { get; } = timestamp;

    /// <summary>
    /// ToString for better debugging.
    /// </summary>
    /// <returns></returns>
    public override string ToString() 
        => $"{nameof(Timestamped<T>)}({new DateTime(CacheTimestamp):O} / {CacheTimestamp})={Value}";
}
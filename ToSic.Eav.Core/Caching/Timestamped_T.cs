using ToSic.Eav.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Caching;

/// <summary>
/// WIP 13.11 - object container with additional timestamp
/// </summary>
/// <typeparam name="T">The type which is enumerated, usually an <see cref="IEntity"/></typeparam>
[PrivateApi("this is just fyi")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Timestamped<T>: ITimestamped
{
    /// <summary>
    /// The cached object/result
    /// </summary>
    public T Value { get; }
        
    /// <summary>
    /// Initialized a new list which depends on another source
    /// </summary>
    public Timestamped(T value, long timestamp)
    {
        Value = value;
        CacheTimestamp = timestamp;
    }


    /// <inheritdoc />
    public long CacheTimestamp { get; }
}
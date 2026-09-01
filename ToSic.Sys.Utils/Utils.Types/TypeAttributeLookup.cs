using System.Collections.Concurrent;

namespace ToSic.Sys.Utils.Types;

/// <summary>
/// Helper to get information on [Attribute]s of a class - incl. helper methods to process any data found before caching.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <remarks>
/// It will cache the result so next time is faster.
/// </remarks>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class TypeAttributeLookup<TValue>
{
    public TValue Get<TCustom, TAttribute>(Func<TAttribute?, TValue> func)
        where TCustom : class
        where TAttribute : Attribute
    {
        return Get(typeof(TCustom), func);
    }

    public TValue Get<TAttribute>(Type type, Func<TAttribute?, TValue> func)
        where TAttribute : Attribute
    {
        // Check cache if already done
        if (_cache.TryGetValue(type, out var typeName))
        {
    #if DEBUG // toggle debug during testing to inform that we used the cache
            UsedCache = true;
    #endif
            return typeName;
        }

#if DEBUG // Tell debug/testing that we did not use the cache yet
        UsedCache = false;
#endif

        // Try to get attribute as specified
        var attribute = type.GetDirectlyAttachedAttribute<TAttribute>();

        // Call the passed in function to extract the values
        typeName = func(attribute);

        // Store result so next time is faster, without reflection
        _cache.GetOrAdd(type, typeName);

        // return result
        return typeName;
    }
    private readonly ConcurrentDictionary<Type, TValue> _cache = new();

#if DEBUG
    /// <summary>
    /// For debugging only, inform if the last access used the cache or not.
    /// </summary>
    public bool UsedCache = false;
#endif
}
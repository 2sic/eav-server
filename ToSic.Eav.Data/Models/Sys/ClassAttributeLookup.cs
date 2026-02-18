using System.Collections.Concurrent;

namespace ToSic.Eav.Models.Sys;

/// <summary>
/// Helper to get attribute values of a class.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <remarks>
/// It will cache the result so next time is faster.
/// </remarks>
internal class ClassAttributeLookup<TValue>
{
    internal TValue Get<TCustom, TAttribute>(Func<TAttribute?, TValue> func)
        where TCustom : class
        where TAttribute : Attribute
    {
        return Get(typeof(TCustom), func);
    }

    internal TValue Get<TAttribute>(Type type, Func<TAttribute?, TValue> func)
        where TAttribute : Attribute
    {
        // Check cache if already done
#if DEBUG
        UsedCache = true;
#endif
        if (_cache.TryGetValue(type, out var typeName))
            return typeName;

#if DEBUG
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
    public bool UsedCache = false;
#endif
}
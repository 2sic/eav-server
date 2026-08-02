using System.Collections.Immutable;
using static System.StringComparer;

namespace ToSic.Sys.Utils;

// ReSharper disable once InconsistentNaming
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class DictionaryExtensions
{
    /// <summary>
    /// Detect if a dictionary is using InvariantCultureIgnoreCase or OrdinalIgnoreCase.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="original"></param>
    /// <returns></returns>
    /// <remarks>
    /// Works for Dictionary and ImmutableDictionary, but NOT for ReadOnlyDictionary, which doesn't seem to expose its comparer.
    /// </remarks>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool IsIgnoreCase<T>(this IDictionary<string, T> original)
        => original.GetComparer().IsIgnoreCase();
    
    internal static IEqualityComparer<string> GetComparer<T>(this IDictionary<string, T> original)
        => original switch
        {
            Dictionary<string, T> dic => dic.Comparer,
            ImmutableDictionary<string, T> dicIm => dicIm.KeyComparer,
            _ => EqualityComparer<string>.Default
        };

    private static bool IsIgnoreCase(this IEqualityComparer<string> comparer)
        => Equals(comparer, InvariantCultureIgnoreCase) || Equals(comparer, OrdinalIgnoreCase);

    private static bool IsInvIgnoreCase(this IEqualityComparer<string> comparer)
        => Equals(comparer, InvariantCultureIgnoreCase);


    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IDictionary<string, T> ToInvariant<T>(this IDictionary<string, T> original)
    {
        // Bypass if it's already doing this - can only be checked on "real" dictionaries
        if (original is Dictionary<string, T> originalDic)
            return originalDic.ToInvariant();
        return new Dictionary<string, T>(original, InvariantCultureIgnoreCase);
    }


    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static Dictionary<string, T> ToInvariant<T>(this Dictionary<string, T> original)
        => original.Comparer.IsInvIgnoreCase()
            ? original
            : original.ToInvIgnoreCaseCopy();


    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IImmutableDictionary<string, T> ToImmutableInvIgnoreCase<T>(this IDictionary<string, T> original)
        => original is ImmutableDictionary<string, T> im && im.KeyComparer.IsInvIgnoreCase()
            ? im
            : original.ToImmutableDictionary(InvariantCultureIgnoreCase);


    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static Dictionary<string, T> ToInvIgnoreCaseCopy<T>(this IDictionary<string, T> original)
        => new(original, InvariantCultureIgnoreCase);


    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IDictionary<string, T> ToEditableIgnoreCase<T>(this IReadOnlyDictionary<string, T> original)
        => original.ToDictionary(pair => pair.Key, pair => pair.Value, InvariantCultureIgnoreCase);

    /// <summary>
    /// Convert a string-object dictionary to a string-string dictionary, filtering out null-object.
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(original))]
    public static ImmutableDictionary<string, string>? ToDicStringStringImInv(this IDictionary<string, object?> original)
    {
        return original
            .Where(pair => pair.Value != null)
            .ToImmutableDictionary(
                pair => pair.Key,
                pair => pair.Value!.ToString()!,
                InvariantCultureIgnoreCase
            );
    }


    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool TryGetTyped<TResult, TKey, TValue>(this IDictionary<TKey, TValue>? source, TKey key, out TResult? result)
    {
        result = default;
        if (source == null)
            return false;
        if (!source.TryGetValue(key, out var innerResult))
            return false;
        //if (innerResult == null) return false;
        if (innerResult is not TResult typed)
            return false;
        result = typed;
        return true;
    }

    // Note: never tested yet, just think it makes sense, be careful when you first use this
    // Created 2026-08-02 2dm
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static TResult GetTypedOrFallback<TResult, TKey, TValue>(this IDictionary<TKey, TValue>? source, TKey key, TResult fallback)
    {
        if (source == null)
            return fallback;
        if (!source.TryGetValue(key, out var innerResult))
            return fallback;
        if (innerResult is not TResult typed)
            return fallback;
        return typed;
    }
    
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> factory)
    {
        if (dict.TryGetValue(key, out var val))
            return val;
        val = factory();
        dict[key] = val;
        return val;
    }
    
    public static string GetValueOrKey(this IDictionary<string, string> dic, string key)
        => dic.TryGetValue(key, out var value) && value != null ? value : key;

    public static IDictionary<string, TValue> FilterOutKeys<TValue>(this IDictionary<string, TValue> dic, IEnumerable<string> keysToRemove)
    {
        var keys = keysToRemove.ToHashSet();
        return dic
            .Where(pair => !keys.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, dic.GetComparer());
    }
}
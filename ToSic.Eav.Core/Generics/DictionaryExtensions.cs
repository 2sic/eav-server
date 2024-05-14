using System.Collections.Immutable;
using static System.StringComparer;

namespace ToSic.Eav.Generics;

// ReSharper disable once InconsistentNaming
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class DictionaryExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IDictionary<string, T> ToInvariant<T>(this IDictionary<string, T> original)
    {
        // Bypass if it's already doing this - can only be checked on "real" dictionaries
        if (original is Dictionary<string, T> originalDic)
            return originalDic.ToInvariant();
        return new Dictionary<string, T>(original, InvariantCultureIgnoreCase);
    }
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IImmutableDictionary<string, T> ToImmutableInvariant<T>(this IDictionary<string, T> original)
    {
        // Bypass if it's already doing this - can only be checked on "real" dictionaries
        return original.ToImmutableDictionary(InvariantCultureIgnoreCase);
    }
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IDictionary<string, T> ToEditable<T>(this IReadOnlyDictionary<string, T> original)
    {
        return original.ToDictionary(pair => pair.Key, pair => pair.Value, InvariantCultureIgnoreCase);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static Dictionary<string, T> ToInvariant<T>(this Dictionary<string, T> original)
    {
        // Bypass if it's already doing this
        if (Equals(original.Comparer, InvariantCultureIgnoreCase) || Equals(original.Comparer, OrdinalIgnoreCase))
            return original;
        return original.ToInvariantCopy();
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static Dictionary<string, T> ToInvariantCopy<T>(this IDictionary<string, T> original) =>
        new(original, InvariantCultureIgnoreCase);

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool TryGetTyped<TResult, TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, out TResult result)
    {
        result = default;
        if (source == null) return false;
        if (!source.TryGetValue(key, out var innerResult)) return false;
        //if (innerResult == null) return false;
        if (innerResult is not TResult typed) return false;
        result = typed;
        return true;
    }
}
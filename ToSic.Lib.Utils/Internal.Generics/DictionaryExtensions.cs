using System.Collections.Immutable;
using static System.StringComparer;

namespace ToSic.Eav.Internal.Generics;

// ReSharper disable once InconsistentNaming
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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
    public static bool IsIgnoreCase<T>(this IDictionary<string, T> original)
        => original is Dictionary<string, T> dic && dic.Comparer.IsIgnoreCase()
        || original is ImmutableDictionary<string, T> dicIm && dicIm.KeyComparer.IsIgnoreCase();

    private static bool IsIgnoreCase(this IEqualityComparer<string> comparer)
        => Equals(comparer, InvariantCultureIgnoreCase) || Equals(comparer, OrdinalIgnoreCase);

    private static bool IsInvIgnoreCase(this IEqualityComparer<string> comparer)
        => Equals(comparer, InvariantCultureIgnoreCase);


    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IDictionary<string, T> ToInvariant<T>(this IDictionary<string, T> original)
    {
        // Bypass if it's already doing this - can only be checked on "real" dictionaries
        if (original is Dictionary<string, T> originalDic)
            return originalDic.ToInvariant();
        return new Dictionary<string, T>(original, InvariantCultureIgnoreCase);
    }


    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static Dictionary<string, T> ToInvariant<T>(this Dictionary<string, T> original)
        => original.Comparer.IsInvIgnoreCase()
            ? original
            : original.ToInvIgnoreCaseCopy();


    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IImmutableDictionary<string, T> ToImmutableInvIgnoreCase<T>(this IDictionary<string, T> original)
        => original is ImmutableDictionary<string, T> im && im.KeyComparer.IsInvIgnoreCase()
            ? im
            : original.ToImmutableDictionary(InvariantCultureIgnoreCase);


    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static Dictionary<string, T> ToInvIgnoreCaseCopy<T>(this IDictionary<string, T> original)
        => new(original, InvariantCultureIgnoreCase);


    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IDictionary<string, T> ToEditableInIgnoreCase<T>(this IReadOnlyDictionary<string, T> original)
        => original.ToDictionary(pair => pair.Key, pair => pair.Value, InvariantCultureIgnoreCase);


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

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> factory)
    {
        if (dict.TryGetValue(key, out var val))
            return val;
        val = factory();
        dict[key] = val;
        return val;
    }
}
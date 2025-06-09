using System.Collections.Frozen;
using System.Collections.Immutable;

namespace ToSic.Sys.Performance;
public static class SysPerfSettings
{
    public static bool PreferArray = false;
    public static bool PreferFrozen = false;

    public static IList<T> ToListOpt<T>(this IEnumerable<T> source)
        => PreferArray
            ? source.ToArray()
            : source.ToList();

    public static IList<T> ToListOptSafe<T>(this IEnumerable<T>? source)
        => source == null
            ? new List<T>()
            : PreferArray
                ? source.ToArray()
                : source.ToList();

    public static IImmutableList<T> ToImmutableSafe<T>(this IEnumerable<T>? source)
        => source == null
            ? ImmutableList<T>.Empty
            : PreferArray
                ? source.ToImmutableArray()
                : source.ToImmutableList();


    public static IReadOnlyDictionary<TKey, TValue> ToImmutableDicSafe<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector,
        IEqualityComparer<TKey>? keyComparer
    ) where TKey : notnull
    {
        return source == null
            ? ImmutableDictionary<TKey, TValue>.Empty
            : PreferFrozen
                ? source.ToFrozenDictionary(keySelector, elementSelector, keyComparer)
                : source.ToImmutableDictionary(keySelector, elementSelector, keyComparer);
    }
}

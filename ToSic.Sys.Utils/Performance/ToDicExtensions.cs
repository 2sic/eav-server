using System.Collections.Frozen;
using System.Collections.Immutable;

namespace ToSic.Sys.Performance;
public static class ToDicExtensions
{
    public static IReadOnlyDictionary<TKey, TValue> ToImmutableDicSafe<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector,
        IEqualityComparer<TKey>? keyComparer
    ) where TKey : notnull
    {
        return source == null
            ? ImmutableDictionary<TKey, TValue>.Empty
            : SysPerfSettings.PreferFrozen
                ? source.ToFrozenDictionary(keySelector, elementSelector, keyComparer)
                : source.ToImmutableDictionary(keySelector, elementSelector, keyComparer);
    }
}

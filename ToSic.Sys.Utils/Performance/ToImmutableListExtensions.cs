using System.Collections.Immutable;

namespace ToSic.Sys.Performance;
public static class ToImmutableListExtensions
{
    public static IImmutableList<T> ToImmutableOpt<T>(this IEnumerable<T> source)
        => SysPerfSettings.PreferArray
                ? source.ToImmutableArray()
                : source.ToImmutableList();

    public static IImmutableList<T> ToImmutableSafe<T>(this IEnumerable<T>? source)
        => source == null
            ? ImmutableList<T>.Empty
            : SysPerfSettings.PreferArray
                ? source.ToImmutableArray()
                : source.ToImmutableList();

}

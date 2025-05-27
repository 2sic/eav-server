namespace ToSic.Lib.Internal.Generics;

/// <summary>
/// Taken from https://github.com/morelinq/MoreLINQ/blob/master/MoreLinq/DistinctBy.cs
/// </summary>
partial class EnumerableExtensions
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool SafeAny<TSource>(
        [NotNullWhen(true)]
        this IEnumerable<TSource>? source
    )
        => source?.Any() == true;

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool SafeNone<TSource>(
        [NotNullWhen(false)]
        this IEnumerable<TSource>? source
    )
        => source?.Any() != true;

}
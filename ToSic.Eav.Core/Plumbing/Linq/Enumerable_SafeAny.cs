namespace ToSic.Eav.Plumbing;

/// <summary>
/// Taken from https://github.com/morelinq/MoreLINQ/blob/master/MoreLinq/DistinctBy.cs
/// </summary>
partial class EnumerableExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool SafeAny<TSource>(this IEnumerable<TSource> source) => source?.Any() == true;

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool SafeNone<TSource>(this IEnumerable<TSource> source) => source?.Any() != true;

}
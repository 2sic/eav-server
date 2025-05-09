namespace ToSic.Lib.Internal.Generics;

/// <summary>
/// Taken from https://github.com/morelinq/MoreLINQ/blob/master/MoreLinq/DistinctBy.cs
/// </summary>
partial class EnumerableExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool SafeAny<TSource>(
#if !NETFRAMEWORK
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
        this IEnumerable<TSource>? source
    )
        => source?.Any() == true;

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool SafeNone<TSource>(
#if !NETFRAMEWORK
        [System.Diagnostics.CodeAnalysis.NotNullWhen(false)]
#endif
        this IEnumerable<TSource>? source
    )
        => source?.Any() != true;

}
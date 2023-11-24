using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Plumbing;

/// <summary>
/// Taken from https://github.com/morelinq/MoreLINQ/blob/master/MoreLinq/DistinctBy.cs
/// </summary>
public static partial class EnumerableExtensions
{
    public static bool SafeAny<TSource>(this IEnumerable<TSource> source) => source?.Any() == true;
    public static bool SafeNone<TSource>(this IEnumerable<TSource> source) => source?.Any() != true;

}
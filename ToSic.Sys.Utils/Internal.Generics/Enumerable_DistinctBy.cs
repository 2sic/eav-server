namespace ToSic.Lib.Internal.Generics;

/// <summary>
/// Taken from https://github.com/morelinq/MoreLINQ/blob/master/MoreLinq/DistinctBy.cs
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static partial class EnumerableExtensions
{
    // Note: seems .net 6 already has this
#if !NET6_0_OR_GREATER

    /// <summary>
    /// Returns all distinct elements of the given source, where "distinctness"
    /// is determined via a projection and the default equality comparer for the projected type.
    /// </summary>
    /// <remarks>
    /// This operator uses deferred execution and streams the results, although
    /// a set of already-seen keys is retained. If a key is seen multiple times,
    /// only the first element with that key is returned.
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence</typeparam>
    /// <typeparam name="TKey">Type of the projected element</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="keySelector">Projection for determining "distinctness"</param>
    /// <returns>A sequence consisting of distinct elements from the source sequence,
    /// comparing them by the specified key projection.</returns>

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector)
    {
        return source.DistinctBy(keySelector, null);
    }
#endif
    /// <summary>
    /// Returns all distinct elements of the given source, where "distinctness"
    /// is determined via a projection and the specified comparer for the projected type.
    /// </summary>
    /// <remarks>
    /// This operator uses deferred execution and streams the results, although
    /// a set of already-seen keys is retained. If a key is seen multiple times,
    /// only the first element with that key is returned.
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence</typeparam>
    /// <typeparam name="TKey">Type of the projected element</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="keySelector">Projection for determining "distinctness"</param>
    /// <param name="comparer">The equality comparer to use to determine whether or not keys are equal.
    /// If null, the default equality comparer for <c>TSource</c> is used.</param>
    /// <returns>A sequence consisting of distinct elements from the source sequence,
    /// comparing them by the specified key projection.</returns>

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));

        return _(); IEnumerable<TSource> _()
        {
            var knownKeys = new HashSet<TKey>(comparer);
            foreach (var element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                    yield return element;
            }
        }
    }
}
using System.Diagnostics.CodeAnalysis;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Persistence.Efc.Sys;
public static class EfcExtensions
{
    public static IQueryable<TEntity> AsNoTrackingOptional<TEntity>(
        [NotNull] this IQueryable<TEntity> source,
        ISysFeaturesService featuresSvc,
        bool preferUntracked = true)
        where TEntity : class
    {
        return preferUntracked && featuresSvc.IsEnabled(BuiltInFeatures.DatabaseTrackingOptimized)
            ? source.AsNoTrackingWithIdentity()
            : source;
    }

    /// <summary>
    /// Returns a new query where the entities will not be tracked by the context,
    /// and in new EFCore repeated rows with the same key will be represented by the same object instance in the result.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    /// <remarks>
    /// fix efcore 10 exception in Oqtane: "The instance of entity type 'TsDynDataDimension' cannot be tracked because another instance with the key value '{DimensionId: N}' is already being tracked."
    /// </remarks>
    public static IQueryable<TEntity> AsNoTrackingWithIdentity<TEntity>(
        this IQueryable<TEntity> source)
        where TEntity : class
        =>
#if NETFRAMEWORK
            source.AsNoTracking();
#else
            // This keeps the query untracked, but ensures repeated rows with the same key are represented by the same object instance in the result.
            source.AsNoTrackingWithIdentityResolution();
#endif
}

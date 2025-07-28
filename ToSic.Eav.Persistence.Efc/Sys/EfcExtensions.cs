using System.Diagnostics.CodeAnalysis;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Persistence.Efc.Sys;
public static class EfcExtensions
{
    public static IQueryable<TEntity> AsNoTrackingOptional<TEntity>(
        [NotNull] this IQueryable<TEntity> source,
        ISysFeaturesService featuresSvc)
        where TEntity : class
    {
        return featuresSvc.IsEnabled(BuiltInFeatures.DbOptimizeTracking)
            ? source.AsNoTracking()
            : source;
    }
}

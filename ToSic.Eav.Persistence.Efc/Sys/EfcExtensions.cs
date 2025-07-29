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
        return preferUntracked && featuresSvc.IsEnabled(BuiltInFeatures.DbOptimizeTracking)
            ? source.AsNoTracking()
            : source;
    }
}

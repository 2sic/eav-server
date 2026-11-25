
using ToSic.Eav.Persistence.Efc.Sys.DbModels;
using ToSic.Eav.Persistence.Efc.Sys.Services;

namespace ToSic.Eav.Persistence.Efc.Sys.Values;

internal class ValueQueries(EfcAppLoaderService appLoader, ILog parentLog): HelperBase(parentLog, "Efc.ValQry")
{
    /// <remarks>
    /// Improved 2025-04-28 for v20 to really just get the values we need, seems to be ca. 50% faster.
    /// </remarks>
    internal IQueryable<TsDynDataValue> ChunkValuesQuery(ICollection<int> entityIds)
    {
        var l = Log.IfSummary(appLoader.LogSettings).Fn<IQueryable<TsDynDataValue>>(timer: true);

        //var dimensions = context.TsDynDataDimensions.ToList(); // materialise (very fast)

        var values = appLoader.Context.TsDynDataValues
            .Where(v => entityIds.Contains(v.EntityId))
            .Include(v => v.Attribute)
            .Include(v => v.TsDynDataValueDimensions)
            .ThenInclude(d => d.Dimension);

        return l.Return(values);
    }

}
using ToSic.Eav.Persistence.Efc.Sys.DbContext;
using ToSic.Eav.Persistence.Efc.Sys.DbModels;
using ToSic.Sys.Capabilities.Features;
using EavDbContext = ToSic.Eav.Persistence.Efc.Sys.DbContext.EavDbContext;

namespace ToSic.Eav.Persistence.Efc.Sys.Entities;

internal class EntityQueries(EavDbContext db, ISysFeaturesService featuresSvc, ILog parentLog) : HelperBase(parentLog, "Efc.EntQry")
{
    /// <summary>
    /// Get entities of a specific app - all or just a selection.
    /// </summary>
    /// <param name="appId">The App ID</param>
    /// <param name="entityIds">List of specific entities to load; usually when doing partial updates.</param>
    /// <param name="filterJsonType">Optional type-name to filter; will only work for JSON types (system provided content-types) as their definition is in another DB column.</param>
    /// <returns></returns>
    internal IQueryable<TsDynDataEntity> EntitiesOfAppQuery(int appId, int[] entityIds, string? filterJsonType = null)
    {
        var filterIds = entityIds.Length > 0;
        var filterByType = filterJsonType != null;
        var l = Log.Fn<IQueryable<TsDynDataEntity>>(
            $"app: {appId}, ids: {entityIds.Length}, filter: {filterIds}; {nameof(filterJsonType)}: '{filterJsonType}'",
            timer: true);

        var query = db.TsDynDataEntities
            .AsNoTrackingOptional(featuresSvc)
            .Where(e => e.AppId == appId)
            .Where(e => e.TransDeletedId == null && e.ContentTypeNavigation.TransDeletedId == null);

        l.A("initial query created");

        // filter by EntityIds (if set)
        if (filterIds)
            query = query.Where(e => Enumerable.Contains(entityIds, e.EntityId));
        l.A($"entityId filter: {(filterIds ? "enabled" : "none")}");

        if (filterByType)
            query = query.Where(e => e.ContentType == filterJsonType);
        l.A($"type filter: {(filterByType ? "enabled" : "none")}");

        return l.Return(query);
    }

    public IQueryable<TsDynDataEntity> EntitiesOfAdditionalDrafts(int[] publishIds)
    {
        var l = Log.Fn<IQueryable<TsDynDataEntity>>(timer: true);

        var relatedIds = db.TsDynDataEntities
            .AsNoTrackingOptional(featuresSvc)
            .Where(e =>
                e.PublishedEntityId.HasValue
                && !e.IsPublished
                && Enumerable.Contains(publishIds, e.EntityId)
                && !Enumerable.Contains(publishIds, e.PublishedEntityId.Value)
                && e.TransDeletedId == null
            );

        return l.ReturnAsOk(relatedIds);
    }

}
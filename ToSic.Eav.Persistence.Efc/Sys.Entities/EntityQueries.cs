namespace ToSic.Eav.Persistence.Efc;

internal class EntityQueries(EavDbContext db, ILog parentLog) : HelperBase(parentLog, "Efc.EntQry")
{
    internal IQueryable<TsDynDataEntity> EntitiesOfAppQuery(int appId, int[] entityIds, string filterType = null)
    {
        var filterIds = entityIds.Length > 0;
        var filterByType = filterType != null;
        var l = Log.Fn<IQueryable<TsDynDataEntity>>(
            $"app: {appId}, ids: {entityIds.Length}, filter: {filterIds}; {nameof(filterType)}: '{filterType}'",
            timer: true);

        var query = db.TsDynDataEntities
            .Where(e => e.AppId == appId)
            .Where(e => e.TransDeletedId == null && e.ContentTypeNavigation.TransDeletedId == null);

        l.A("initial query created");

        // filter by EntityIds (if set)
        if (filterIds)
            query = query.Where(e => Enumerable.Contains(entityIds, e.EntityId));
        l.A($"entityId filter: {(filterIds ? "enabled" : "none")}");

        if (filterByType)
            query = query.Where(e => e.ContentType == filterType);
        l.A($"type filter: {(filterByType ? "enabled" : "none")}");

        return l.Return(query);
    }

    public IQueryable<TsDynDataEntity> EntitiesOfAdditionalDrafts(int[] publishIds)
    {
        var l = Log.Fn<IQueryable<TsDynDataEntity>>(timer: true);

        var relatedIds = db.TsDynDataEntities
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
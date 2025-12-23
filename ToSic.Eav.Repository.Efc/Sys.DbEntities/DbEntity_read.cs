using ToSic.Eav.Persistence.Efc.Sys;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

partial class DbEntity
{
    private IQueryable<TsDynDataEntity> GetEntityQuery(bool preferUntracked = false)
    {
        LogDetails.A(nameof(GetEntityQuery));

        return DbStore.SqlDb.TsDynDataEntities.AsNoTrackingOptional(DbStore.Features, preferUntracked)
            .Include(e => e.RelationshipsWithThisAsParent)
            .Include(e => e.RelationshipsWithThisAsChild)
            .Include(e => e.TsDynDataValues)
            .ThenInclude(v => v.TsDynDataValueDimensions)
            .ThenInclude(d => d.Dimension);
    }

    /// <summary>
    /// Get a single Entity by EntityId
    /// </summary>
    /// <returns>Entity or throws InvalidOperationException</returns>
    internal TsDynDataEntity GetDbEntityFull(int entityId, bool preferUntracked = false)
    {
        var l = LogDetails.Fn<TsDynDataEntity>($"Get {entityId}");
        var found = GetEntityQuery().Single(e => e.EntityId == entityId);
        return l.ReturnAsOk(found);
    }

    /// <summary>
    /// Get a single Entity by EntityId
    /// </summary>
    /// <returns>Entity or throws InvalidOperationException</returns>
    internal TsDynDataEntity GetDbEntityStub(int entityId, bool preferUntracked = false)
    {
        var l = LogDetails.Fn<TsDynDataEntity>($"Get {entityId}");
        var found = DbStore.SqlDb.TsDynDataEntities
            .AsNoTrackingOptional(DbStore.Features, preferUntracked)
            .Single(e => e.EntityId == entityId);
        return l.ReturnAsOk(found);
    }

    /// <summary>
    /// Get a single Entity by EntityId
    /// </summary>
    /// <returns>Entity or throws InvalidOperationException</returns>
    internal TsDynDataEntity[] GetDbEntitiesWithChildren(int[] repositoryIds, bool preferUntracked = false)
    {
        var l = LogDetails.Fn<TsDynDataEntity[]>($"Get {repositoryIds.Length}", timer: true);
        // commented because of https://github.com/npgsql/efcore.pg/issues/3461, we can go back with net10.0
        // var found = EntityQuery.Where(e => repositoryIds.Contains(e.EntityId)).ToArray();
        //var found = EntityQuery
        //    .Where(e => Enumerable.Contains(repositoryIds, e.EntityId))
        //    .ToArray();

        // var found = DbContext.SqlDb.ToSicEavEntities.Where(e => repositoryIds.Contains(e.EntityId)).ToArray();
        var found = DbStore.SqlDb.TsDynDataEntities
            .AsNoTrackingOptional(DbStore.Features, preferUntracked)
            .Include(e => e.RelationshipsWithThisAsParent)
            .Where(e => Enumerable.Contains(repositoryIds, e.EntityId))
            .ToArray();
        return l.Return(found, found.Length.ToString());
    }

    public List<TsDynDataEntity> GetDbEntitiesFullUntracked(int[] entityIds)
    {
        // 2025-07-28 2dm removed the Include, as they are currently in the EntityQuery - though not sure if relevant
        var queryBase = GetEntityQuery(preferUntracked: true);

        // commented because of https://github.com/npgsql/efcore.pg/issues/3461, we can go back with net10.0
        // => IncludeMultiple(EntityQuery, includes).Where(e => entityIds.Contains(e.EntityId)).ToList();
        var result = queryBase
            .Where(e => Enumerable.Contains(entityIds, e.EntityId))
            .ToList();
        return result;
    }

    /// <summary>
    /// Get a single Entity by EntityGuid. Ensure it's not deleted and has context's AppId
    /// </summary>
    /// <returns>Entity or throws InvalidOperationException</returns>
    internal TsDynDataEntity GetStandaloneDbEntityStub(Guid entityGuid, bool preferUntracked = false)
        // GetEntity should never return a draft entity that has a published version
    {
        var x = GetEntityStubsByGuid(entityGuid, preferUntracked);
        return x.Single(e => !e.PublishedEntityId.HasValue);
    }


    //internal IQueryable<ToSicEavEntities> GetEntitiesByGuid(Guid entityGuid)
    //    => EntityQuery.Where(e => e.EntityGuid == entityGuid
    //                              && !e.TransDeletedId.HasValue
    //                              && !e.AttributeSet.TransDeletedId.HasValue
    //                                // commented because of https://github.com/npgsql/efcore.pg/issues/3461, we can go back with net10.0
    //                                // && DbContext.AppIds.Contains(e.AppId));
    //                                && Enumerable.Contains(DbContext.AppIds, e.AppId));

    internal IQueryable<TsDynDataEntity> GetEntityStubsByGuid(Guid entityGuid, bool preferUntracked = false)
        //=> EntityQuery
        => DbStore.SqlDb.TsDynDataEntities
            .AsNoTrackingOptional(DbStore.Features, preferUntracked)
            .Where(e => e.EntityGuid == entityGuid
                                       && e.TransDeletedId == null
                                       && !e.ContentTypeNavigation.TransDeletedId.HasValue
                                       // commented because of https://github.com/npgsql/efcore.pg/issues/3461, we can go back with net10.0
                                       // && DbContext.AppIds.Contains(e.AppId));
                                       && Enumerable.Contains(DbStore.AppIds, e.AppId)
            );

    /// <summary>
    /// Get a single Entity by EntityGuid. Ensure it's not deleted and has context's AppId
    /// </summary>
    /// <returns>Entity or throws InvalidOperationException</returns>
    internal Dictionary<Guid, int> GetMostCurrentDbEntities(Guid[] entityGuids, bool preferUntracked = false)
    {
        // GetEntity should never return a draft entity that has a published version
        var l = LogDetails.Fn<Dictionary<Guid, int>>($"Guids: {entityGuids.Length}; [{string.Join(",", entityGuids)}]; preferUntracked: {preferUntracked}", timer: true);

        var getEntityQuery = GetEntityStubsByGuid(entityGuids, preferUntracked);
        var dbEntityList = getEntityQuery.ToList(); // necessary for EF 3 - before GroupBy so it's then done in memory and not in SQL
        l.A($"SQL found {dbEntityList.Count} entities with IDs: [{string.Join(",", dbEntityList.Select(e => e.EntityId))}]");

        try
        {
            var result = dbEntityList
                .GroupBy(e => e.EntityGuid)
                .ToDictionary(
                    g => g.Key,
                    g => g.Single(e => !e.PublishedEntityId.HasValue).EntityId
                );

            return l.Return(result, result.Count.ToString());
        }
        catch (InvalidOperationException e)
        {
            l.Ex(e);
            throw;
        }

    }

    // 2020-10-07 2dm experiment with fewer requests
    private IQueryable<TsDynDataEntity> GetEntityStubsByGuid(Guid[] entityGuid, bool preferUntracked = false)
        //=> EntityQuery
        => DbStore.SqlDb.TsDynDataEntities
            .AsNoTrackingOptional(DbStore.Features, preferUntracked)
            .Where(e =>
                        // commented because of https://github.com/npgsql/efcore.pg/issues/3461, we can go back with net10.0
                        // entityGuid.Contains(e.EntityGuid)
                        Enumerable.Contains(entityGuid, e.EntityGuid)
                        && e.TransDeletedId == null
                        && e.ContentTypeNavigation.TransDeletedId == null
                        // commented because of https://github.com/npgsql/efcore.pg/issues/3461, we can go back with net10.0
                        // && DbContext.AppIds.Contains(e.AppId)
                        && Enumerable.Contains(DbStore.AppIds, e.AppId)
            );


    /// <summary>
    /// Test whether Entity exists on current App and is not deleted
    /// </summary>
    internal bool EntityExists(Guid entityGuid, bool preferUntracked = false) => GetEntityStubsByGuid(entityGuid, preferUntracked).Any();

}
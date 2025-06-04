namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

partial class DbEntity
{

    private IQueryable<TsDynDataEntity> EntityQuery
    {
        get
        {
            DbContext.Log.A(nameof(EntityQuery));
            return DbContext.SqlDb.TsDynDataEntities
                .Include(e => e.RelationshipsWithThisAsParent)
                .Include(e => e.RelationshipsWithThisAsChild)
                .Include(e => e.TsDynDataValues)
                .ThenInclude(v => v.TsDynDataValueDimensions)
                .ThenInclude(d => d.Dimension);
        }
    }

    /// <summary>
    /// Get a single Entity by EntityId
    /// </summary>
    /// <returns>Entity or throws InvalidOperationException</returns>
    internal TsDynDataEntity GetDbEntityFull(int entityId)
    {
        var l = DbContext.Log.Fn<TsDynDataEntity>($"Get {entityId}");
        var found = EntityQuery.Single(e => e.EntityId == entityId);
        return l.ReturnAsOk(found);
    }

    /// <summary>
    /// Get a single Entity by EntityId
    /// </summary>
    /// <returns>Entity or throws InvalidOperationException</returns>
    internal TsDynDataEntity GetDbEntityStub(int entityId)
    {
        var l = DbContext.Log.Fn<TsDynDataEntity>($"Get {entityId}");
        var found = DbContext.SqlDb.TsDynDataEntities.Single(e => e.EntityId == entityId);
        return l.ReturnAsOk(found);
    }

    /// <summary>
    /// Get a single Entity by EntityId
    /// </summary>
    /// <returns>Entity or throws InvalidOperationException</returns>
    internal TsDynDataEntity[] GetDbEntitiesWithChildren(int[] repositoryIds)
    {
        var l = DbContext.Log.Fn<TsDynDataEntity[]>($"Get {repositoryIds.Length}", timer: true);
        // commented because of https://github.com/npgsql/efcore.pg/issues/3461, we can go back with net10.0
        // var found = EntityQuery.Where(e => repositoryIds.Contains(e.EntityId)).ToArray();
        //var found = EntityQuery
        //    .Where(e => Enumerable.Contains(repositoryIds, e.EntityId))
        //    .ToArray();

        // var found = DbContext.SqlDb.ToSicEavEntities.Where(e => repositoryIds.Contains(e.EntityId)).ToArray();
        var found = DbContext.SqlDb.TsDynDataEntities
            .Include(e => e.RelationshipsWithThisAsParent)
            .Where(e => Enumerable.Contains(repositoryIds, e.EntityId))
            .ToArray();
        return l.Return(found, found.Length.ToString());
    }

    private List<TsDynDataEntity> GetDbEntities(int[] entityIds)
    {
        var queryBase = EntityQuery
            .Include(e => e.TsDynDataValues)
            .ThenInclude(v => v.TsDynDataValueDimensions);

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
    internal TsDynDataEntity GetStandaloneDbEntityStub(Guid entityGuid)
        // GetEntity should never return a draft entity that has a published version
    {
        var x = GetEntityStubsByGuid(entityGuid);
        return x.Single(e => !e.PublishedEntityId.HasValue);
    }


    //internal IQueryable<ToSicEavEntities> GetEntitiesByGuid(Guid entityGuid)
    //    => EntityQuery.Where(e => e.EntityGuid == entityGuid
    //                              && !e.TransDeletedId.HasValue
    //                              && !e.AttributeSet.TransDeletedId.HasValue
    //                                // commented because of https://github.com/npgsql/efcore.pg/issues/3461, we can go back with net10.0
    //                                // && DbContext.AppIds.Contains(e.AppId));
    //                                && Enumerable.Contains(DbContext.AppIds, e.AppId));

    internal IQueryable<TsDynDataEntity> GetEntityStubsByGuid(Guid entityGuid)
        //=> EntityQuery
        => DbContext.SqlDb.TsDynDataEntities
            .Where(e => e.EntityGuid == entityGuid
                        && !e.TransDeletedId.HasValue
                        && !e.ContentTypeNavigation.TransDeletedId.HasValue
                        // commented because of https://github.com/npgsql/efcore.pg/issues/3461, we can go back with net10.0
                        // && DbContext.AppIds.Contains(e.AppId));
                        && Enumerable.Contains(DbContext.AppIds, e.AppId)
            );

    /// <summary>
    /// Get a single Entity by EntityGuid. Ensure it's not deleted and has context's AppId
    /// </summary>
    /// <returns>Entity or throws InvalidOperationException</returns>
    internal Dictionary<Guid, int> GetMostCurrentDbEntities(Guid[] entityGuids)
    {
        // GetEntity should never return a draft entity that has a published version
        var l = Log.Fn<Dictionary<Guid, int>>($"Guids: {entityGuids.Length}; [{string.Join(",", entityGuids)}]", timer: true);

        var getEntityQuery = GetEntityStubsByGuid(entityGuids);
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
    private IQueryable<TsDynDataEntity> GetEntityStubsByGuid(Guid[] entityGuid)
        //=> EntityQuery
        => DbContext.SqlDb.TsDynDataEntities
            .Where(e =>
                        // commented because of https://github.com/npgsql/efcore.pg/issues/3461, we can go back with net10.0
                        // entityGuid.Contains(e.EntityGuid)
                        Enumerable.Contains(entityGuid, e.EntityGuid)
                        && e.TransDeletedId == null
                        && e.ContentTypeNavigation.TransDeletedId == null
                        // commented because of https://github.com/npgsql/efcore.pg/issues/3461, we can go back with net10.0
                        // && DbContext.AppIds.Contains(e.AppId)
                        && Enumerable.Contains(DbContext.AppIds, e.AppId)
            );


    /// <summary>
    /// Test whether Entity exists on current App and is not deleted
    /// </summary>
    internal bool EntityExists(Guid entityGuid) => GetEntityStubsByGuid(entityGuid).Any();

}
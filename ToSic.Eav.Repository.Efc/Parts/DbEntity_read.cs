using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts;

partial class DbEntity
{

    private IQueryable<ToSicEavEntities> EntityQuery
    {
        get
        {
            DbContext.Log.A(nameof(EntityQuery));
            return DbContext.SqlDb.ToSicEavEntities
                .Include(e => e.RelationshipsWithThisAsParent)
                .Include(e => e.RelationshipsWithThisAsChild)
                .Include(e => e.ToSicEavValues)
                .ThenInclude(v => v.ToSicEavValuesDimensions)
                .ThenInclude(d => d.Dimension);
        }
    }


    private IQueryable<ToSicEavEntities> IncludeMultiple(IQueryable<ToSicEavEntities> origQuery, string additionalTables)
    {
        additionalTables.Split(',').ToList().ForEach(a => origQuery = origQuery.Include(a.Trim()));
        return origQuery;
    }

    /// <summary>
    /// Get a single Entity by EntityId
    /// </summary>
    /// <returns>Entity or throws InvalidOperationException</returns>
    internal ToSicEavEntities GetDbEntity(int entityId)
    {
        var callLog = DbContext.Log.Fn<ToSicEavEntities>($"Get {entityId}");
        var found = EntityQuery.Single(e => e.EntityId == entityId);
        return callLog.ReturnAsOk(found);
    }

    /// <summary>
    /// Get a single Entity by EntityId
    /// </summary>
    /// <returns>Entity or throws InvalidOperationException</returns>
    internal ToSicEavEntities[] GetDbEntities(int[] repositoryIds)
    {
        var callLog = DbContext.Log.Fn<ToSicEavEntities[]>($"Get {repositoryIds.Length}", timer: true);
        var found = EntityQuery.Where(e => repositoryIds.Contains(e.EntityId)).ToArray();
        return callLog.Return(found, found.Length.ToString());
    }

    //private ToSicEavEntities GetDbEntity(int entityId, string includes)
    //    => IncludeMultiple(EntityQuery, includes).Single(e => e.EntityId == entityId);

    private List<ToSicEavEntities> GetDbEntities(int[] entityIds, string includes)
        => IncludeMultiple(EntityQuery, includes).Where(e => entityIds.Contains(e.EntityId)).ToList();

    /// <summary>
    /// Get a single Entity by EntityGuid. Ensure it's not deleted and has context's AppId
    /// </summary>
    /// <returns>Entity or throws InvalidOperationException</returns>
    internal ToSicEavEntities GetMostCurrentDbEntity(Guid entityGuid)
        // GetEntity should never return a draft entity that has a published version
    {
        var x = GetEntitiesByGuid(entityGuid);
        return x.Single(e => !e.PublishedEntityId.HasValue);
    }


    internal IQueryable<ToSicEavEntities> GetEntitiesByGuid(Guid entityGuid)
        => EntityQuery.Where(e => e.EntityGuid == entityGuid
                                  && !e.ChangeLogDeleted.HasValue
                                  && !e.AttributeSet.ChangeLogDeleted.HasValue
                                  && DbContext.AppIds.Contains(e.AppId));

    /// <summary>
    /// Get a single Entity by EntityGuid. Ensure it's not deleted and has context's AppId
    /// </summary>
    /// <returns>Entity or throws InvalidOperationException</returns>
    internal Dictionary<Guid, int> GetMostCurrentDbEntities(Guid[] entityGuid)
        // GetEntity should never return a draft entity that has a published version
    {
        var callLog = Log.Fn<Dictionary<Guid, int>>(timer: true);
        var result = GetEntitiesByGuid(entityGuid)
            .ToList() // necessary for EF 3 - before GroupBy
            .GroupBy(e => e.EntityGuid)
            .ToDictionary(
                g => g.Key,
                g => g.Single(e => !e.PublishedEntityId.HasValue).EntityId);
        return callLog.Return(result, result.Count.ToString());
    }

    // 2020-10-07 2dm experiment with fewer requests
    internal IQueryable<ToSicEavEntities> GetEntitiesByGuid(Guid[] entityGuid)
        => EntityQuery.Where(e => entityGuid.Contains(e.EntityGuid)
                                  && !e.ChangeLogDeleted.HasValue
                                  && !e.AttributeSet.ChangeLogDeleted.HasValue
                                  && DbContext.AppIds.Contains(e.AppId));


    /// <summary>
    /// Test whether Entity exists on current App and is not deleted
    /// </summary>
    internal bool EntityExists(Guid entityGuid) => GetEntitiesByGuid(entityGuid).Any();

}
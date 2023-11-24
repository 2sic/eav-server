using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts;

internal class DbPublishing : DbPartBase
{
    private readonly DataBuilder _builder;
    public DbPublishing(DbDataController db, DataBuilder builder) : base(db, "Db.Publ")
    {
        _builder = builder;
    }

    /// <summary>
    /// Publish a Draft-Entity
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="draftToPublishForJson">Item to be published as the original, for serialization</param>
    /// <returns>The published Entity</returns>
    internal void PublishDraftInDbEntity(int entityId, IEntity draftToPublishForJson) => Log.Do($"{nameof(entityId)}:{entityId}", l =>
    {
        var unpublishedDbEnt = DbContext.Entities.GetDbEntity(entityId);
        if (!unpublishedDbEnt.IsPublished)
            l.A("found item is draft, will use this to publish");
        else
        {
            l.A("found item is published - will try to find draft");
            // try to get the draft if it exists
            var draftId = GetDraftBranchEntityId(entityId);
            l.A($"found draft: {draftId}");
            if (!draftId.HasValue)
                throw new EntityAlreadyPublishedException($"EntityId {entityId} is already published");
            unpublishedDbEnt = DbContext.Entities.GetDbEntity(draftId.Value);
        }

        // Publish Draft-Entity
        if (!unpublishedDbEnt.PublishedEntityId.HasValue)
        {
            l.A("there was no published (not branched), so will just set this to published");
            unpublishedDbEnt.IsPublished = true;
        }
        // Replace currently published Entity with draft Entity and delete the draft
        else
        {
            var publishedId = unpublishedDbEnt.PublishedEntityId.Value;
            l.A(
                "There is a published item, will update that with the draft-data and delete the draft afterwards");
            var publishedEntity = DbContext.Entities.GetDbEntity(publishedId);
            var json = unpublishedDbEnt.Json;
            var isJson = !string.IsNullOrEmpty(json);
            l.A($"this is a json:{isJson}");

            if (isJson)
            {
                l.A($"Must convert back to entity, to then modify the EntityId. The json: {json}");
                // update the content-id
                var cloneWithPublishedId = _builder.Entity.CreateFrom(draftToPublishForJson, id: publishedId);
                //draftToPublishForJson.ResetEntityId(publishedId);

                var serializer = DbContext.JsonSerializerGenerator.New();
                json = serializer.Serialize(cloneWithPublishedId);

                l.A($"changed - final json: {json}");
            }

            publishedEntity.Json = json; // if it's using the new format
            publishedEntity.ChangeLogModified =
                unpublishedDbEnt.ChangeLogModified; // transfer last-modified date (not to today, but to last edit)
            DbContext.Values.CloneRelationshipsAndSave(unpublishedDbEnt,
                publishedEntity); // relationships need special treatment and intermediate save!
            DbContext.Values.CloneEntitySimpleValues(unpublishedDbEnt, publishedEntity);

            // delete the Draft Entity
            DbContext.Entities.DeleteEntity(unpublishedDbEnt.EntityId, false);
        }

        l.A("About to save...");
        DbContext.SqlDb.SaveChanges();
    });

    /// <summary>
    /// Should clean up branches of this item, and set the one and only as published
    /// </summary>
    /// <param name="draftId"></param>
    /// <param name="newPublishedState"></param>
    /// <param name="publishedEntity"></param>
    /// <returns></returns>
    internal ToSicEavEntities ClearDraftBranchAndSetPublishedState(ToSicEavEntities publishedEntity, int? draftId = null, bool newPublishedState = true)
    {
        Log.A($"clear draft branch for i:{publishedEntity.EntityId}, draft:{draftId}, state:{newPublishedState}");
        var unpublishedEntityId = draftId ?? DbContext.Publishing.GetDraftBranchEntityId(publishedEntity.EntityId);

        // if additional draft exists, must clear that first
        if (unpublishedEntityId != null)
            DbContext.Entities.DeleteEntity(unpublishedEntityId.Value);

        publishedEntity.IsPublished = newPublishedState;

        return publishedEntity;
    }

    /// <summary>
    /// Get Draft EntityId of a Published EntityId. Returns null if there's none.
    /// </summary>
    /// <param name="entityId">EntityId of the Published Entity</param>
    internal int? GetDraftBranchEntityId(int entityId)
    {
        var draftId = DbContext.SqlDb.ToSicEavEntities
            .Where(e => e.PublishedEntityId == entityId && !e.ChangeLogDeleted.HasValue)
            .Select(e => (int?) e.EntityId)
            .SingleOrDefault();
        Log.A($"GetDraftBranchEntityId({entityId}) found {draftId}");
        return draftId;
    }

    /// <summary>
    /// Get Draft EntityId of a Published EntityId. Returns null if there's none.
    /// </summary>
    /// <param name="entityIds">EntityId of the Published Entity</param>
    internal Dictionary<int, int?> GetDraftBranchMap(List<int> entityIds)
    {
        var wrapLog = Log.Fn<Dictionary<int, int?>>($"items: {entityIds.Count}", timer: true);
        var nullList = entityIds.Cast<int?>().ToList();
        var ids = DbContext.SqlDb.ToSicEavEntities
            .Where(e => nullList.Contains(e.PublishedEntityId) && !e.ChangeLogDeleted.HasValue)
            .Select(e => new {e.EntityId, e.PublishedEntityId })
            .ToList();
        // note: distinct is necessary, because new entities all have 0 as the id
        var dic = entityIds.Distinct().ToDictionary(e => e, e => ids.FirstOrDefault(i => i.PublishedEntityId == e)?.EntityId);
        return wrapLog.Return(dic, $"found {ids.Count}");
    }
}
    

internal class EntityAlreadyPublishedException : Exception {
    public EntityAlreadyPublishedException(string message): base(message)
    {}
}
﻿using System;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    internal class DbPublishing : BllCommandBase
    {
        public DbPublishing(DbDataController c) : base(c, "Db.Publ") { }

        /// <summary>
        /// Publish a Draft-Entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="draftToPublishForJson">Item to be published as the original, for serialization</param>
        /// <returns>The published Entity</returns>
        internal ToSicEavEntities PublishDraftInDbEntity(int entityId, IEntity draftToPublishForJson)
        {
            var wrapLog = Log.Call($"{entityId}");
            var unpublishedEntity = DbContext.Entities.GetDbEntity(entityId);
            if (!unpublishedEntity.IsPublished)
                Log.Add("found item is draft, will use this to publish");
            else
            {
                Log.Add("found item is published - will try to find draft");
                // try to get the draft if it exists
                var draftId = GetDraftBranchEntityId(entityId);
                Log.Add($"found draft: {draftId}");
                if (!draftId.HasValue)
                    throw new EntityAlreadyPublishedException($"EntityId {entityId} is already published");
                unpublishedEntity = DbContext.Entities.GetDbEntity(draftId.Value);
            }

            ToSicEavEntities publishedEntity;

            // Publish Draft-Entity
            if (!unpublishedEntity.PublishedEntityId.HasValue)
            {
                Log.Add("there was no published (not branched), so will just set this to published");
                unpublishedEntity.IsPublished = true;
                publishedEntity = unpublishedEntity;
            }
            // Replace currently published Entity with draft Entity and delete the draft
            else
            {
                var publishedId = unpublishedEntity.PublishedEntityId.Value;
                Log.Add("There is a published item, will update that with the draft-data and delete the draft afterwards");
                publishedEntity = DbContext.Entities.GetDbEntity(publishedId);
                var json = unpublishedEntity.Json;
                var isJson = !string.IsNullOrEmpty(json);
                Log.Add($"this is a json:{isJson}");

                // 2020-01-17 2dm - this is the big culprit
                // we're just copying the JSON, and thereby inheriting the EntityId in the draft-json
                // which then doesn't match the old one any more!
                if (isJson)
                {
                    Log.Add($"Must convert back to entity, to then modify the EntityId. The json: {json}");
                    // update the content-id
                    draftToPublishForJson.ResetEntityId(publishedId);
                    var serializer = new JsonSerializer();
                    json = serializer.Serialize(draftToPublishForJson);

                    // Alternative code 2020-01-17, works, but more processing
                    // left here for a while in case we need it after all
                    //var appForSerializer = State.Get(unpublishedEntity.AppId);
                    //var serializer = new JsonSerializer(appForSerializer, Log);
                    ////serializer.Log.LinkTo(Log);
                    //var e = serializer.Deserialize(json) as Entity;
                    //Log.Add($"built entity, will now reset the ID to {publishedId}");
                    //e.ResetEntityId(publishedId);
                    //Log.Add("now re-serialize");
                    //json = serializer.Serialize(e);
                    Log.Add($"changed - final json: {json}");
                }

                publishedEntity.Json = json;  // if it's using the new format
                publishedEntity.ChangeLogModified = unpublishedEntity.ChangeLogModified; // transfer last-modified date (not to today, but to last edit)
                DbContext.Values.CloneRelationshipsAndSave(unpublishedEntity, publishedEntity); // relationships need special treatment and intermediate save!
                DbContext.Values.CloneEntitySimpleValues(unpublishedEntity, publishedEntity);

                // delete the Draft Entity
                DbContext.Entities.DeleteEntity(unpublishedEntity.EntityId, false);
            }

            Log.Add("About to save...");
            DbContext.SqlDb.SaveChanges();
            wrapLog($"ok {entityId}");
            return publishedEntity;
        }

        /// <summary>
        /// Should clean up branches of this item, and set the one and only as published
        /// </summary>
        /// <param name="draftId"></param>
        /// <param name="newPublishedState"></param>
        /// <param name="publishedEntity"></param>
        /// <returns></returns>
        internal ToSicEavEntities ClearDraftBranchAndSetPublishedState(ToSicEavEntities publishedEntity, int? draftId = null, bool newPublishedState = true)
        {
            Log.Add($"clear draft branch for i:{publishedEntity.EntityId}, draft:{draftId}, state:{newPublishedState}");
            // find main Db item and if 
            //var publishedEntity = DbContext.Entities.GetDbEntity(entityId);
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
            Log.Add($"GetDraftBranchEntityId({entityId}) found {draftId}");
            return draftId;
        }
    }
    

    internal class EntityAlreadyPublishedException : Exception {
        public EntityAlreadyPublishedException(string message): base(message)
        {}
    }
}

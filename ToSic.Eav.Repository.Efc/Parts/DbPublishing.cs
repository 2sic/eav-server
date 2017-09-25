using System;
using System.Linq;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    internal class DbPublishing : BllCommandBase
    {
        public DbPublishing(DbDataController c) : base(c, "DbPubl") { }

        /// <summary>
        /// Publish a Draft-Entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns>The published Entity</returns>
        internal ToSicEavEntities PublishDraftInDbEntity(int entityId)
        {
            var unpublishedEntity = DbContext.Entities.GetDbEntity(entityId);
            if (unpublishedEntity.IsPublished)
            {
                // try to get the draft if it exists
                var draftId = GetDraftBranchEntityId(entityId);
                if (!draftId.HasValue)
                    throw new EntityAlreadyPublishedException($"EntityId {entityId} is already published");
                unpublishedEntity = DbContext.Entities.GetDbEntity(draftId.Value);
            }
            ToSicEavEntities publishedEntity;

            // Publish Draft-Entity
            if (!unpublishedEntity.PublishedEntityId.HasValue)
            {
                unpublishedEntity.IsPublished = true;
                publishedEntity = unpublishedEntity;
            }
            // Replace currently published Entity with draft Entity and delete the draft
            else
            {
                publishedEntity = DbContext.Entities.GetDbEntity(unpublishedEntity.PublishedEntityId.Value);
                publishedEntity.ChangeLogModified = unpublishedEntity.ChangeLogModified; // transfer last-modified date (not to today, but to last edit)
                DbContext.Values.CloneRelationshipsAndSave(unpublishedEntity, publishedEntity); // relationships need special treatment and intermediate save!
                DbContext.Values.CloneEntitySimpleValues(unpublishedEntity, publishedEntity);

                // delete the Draft Entity
                DbContext.Entities.DeleteEntity(unpublishedEntity.EntityId, false);
            }

            DbContext.SqlDb.SaveChanges();

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
            => DbContext.SqlDb.ToSicEavEntities
                .Where(e => e.PublishedEntityId == entityId && !e.ChangeLogDeleted.HasValue)
                .Select(e => (int?)e.EntityId)
                .SingleOrDefault();
    }
    

    internal class EntityAlreadyPublishedException : Exception {
        public EntityAlreadyPublishedException(string message): base(message)
        {}
    }
}

using System;
using System.Linq;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    internal class DbPublishing: BllCommandBase
    {
        public DbPublishing(DbDataController c) : base(c) { }

        /// <summary>
        /// Publish a Draft-Entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns>The published Entity</returns>
        public ToSicEavEntities PublishDraftInDbEntity(int entityId)
        {
            var unpublishedEntity = DbContext.Entities.GetDbEntity(entityId);
            if (unpublishedEntity.IsPublished)
            {
                // try to get the draft if it exists
                var draftId = GetDraftEntityId(entityId);
                if (!draftId.HasValue)
                    throw new InvalidOperationException($"EntityId {entityId} is already published");
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
                DbContext.Values.CloneEntityValues(unpublishedEntity, publishedEntity);

                // delete the Draft Entity
                DbContext.Entities.DeleteEntity(unpublishedEntity, false);
            }

            DbContext.SqlDb.SaveChanges();

            return publishedEntity;
        }

        /// <summary>
        /// Should clean up branches of this item, and set the one and only as published
        /// </summary>
        /// <param name="unpublishedEntityId"></param>
        /// <param name="newPublishedState"></param>
        /// <returns></returns>
        public ToSicEavEntities ClearDraftBranchAndSetPublishedState(int unpublishedEntityId, bool newPublishedState = true)
        {
            var unpublishedEntity = DbContext.Entities.GetDbEntity(unpublishedEntityId);

            ToSicEavEntities publishedEntity;

            // Publish Draft-Entity
            if (!unpublishedEntity.PublishedEntityId.HasValue)
            {
                unpublishedEntity.IsPublished = newPublishedState;
                publishedEntity = unpublishedEntity;
            }
            // Replace currently published Entity with draft Entity and delete the draft
            else
            {
                publishedEntity = DbContext.Entities.GetDbEntity(unpublishedEntity.PublishedEntityId.Value);
                publishedEntity.IsPublished = newPublishedState;

                // delete the Draft Entity
                DbContext.Entities.DeleteEntity(unpublishedEntity, false);
            }

            return publishedEntity;
        }

        /// <summary>
        /// Get Draft EntityId of a Published EntityId. Returns null if there's none.
        /// </summary>
        /// <param name="entityId">EntityId of the Published Entity</param>
        internal int? GetDraftEntityId(int entityId)
            => DbContext.SqlDb.ToSicEavEntities
                .Where(e => e.PublishedEntityId == entityId && !e.ChangeLogDeleted.HasValue)
                .Select(e => (int?) e.EntityId)
                .SingleOrDefault();
    }
}

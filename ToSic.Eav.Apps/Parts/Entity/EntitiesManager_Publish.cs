using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        private void PublishWithoutPurge(int entityId)
        {
            Log.A($"PublishWithoutPurge({entityId})");

            // 1. make sure we're publishing the draft, because the entityId might be the published one...
            var contEntity = Parent.Read.Entities.Get(entityId);
            if (contEntity == null)
                Log.A($"Will skip, couldn't find the entity {entityId}");
            else
            {
                Log.A($"found id: {contEntity.EntityId}, " +
                        $"rid: {contEntity.RepositoryId}, isPublished: {contEntity.IsPublished}");

                var maybeDraft = contEntity.IsPublished
                    ? contEntity.GetDraft() ?? contEntity   // if no draft exists, use current
                    : contEntity;// if it isn't published, use current

                var repoId = maybeDraft.RepositoryId;

                Log.A($"publish requested for:{entityId}, " +
                        $"will publish: {repoId} if published false (it's: {maybeDraft.IsPublished})");

                if (!maybeDraft.IsPublished)
                    Parent.DataController.Publishing.PublishDraftInDbEntity(repoId, maybeDraft);
            }

            Log.A($"/PublishWithoutPurge({entityId})");
        }

        /// <summary>
        /// Publish many entities
        /// </summary>
        public void Publish(int[] entityIds)
        {
            Log.A(() => "Publish(" + entityIds.Length + " items [" + string.Join(",", entityIds) + "])");
            foreach (var eid in entityIds)
                try
                {
                    PublishWithoutPurge(eid);
                }
                catch (Repository.Efc.Parts.EntityAlreadyPublishedException) { /* ignore */ }
            // Tell the cache to do a partial update
            _appsCache.Value.Update(Parent, entityIds, Log, _appLoaderTools.Value);
            Log.A("/Publish(...)");
        }

        /// <summary>
        /// Publish a single entity 
        /// </summary>
        public void Publish(int entityId) => Publish(new[] { entityId });


    }
}

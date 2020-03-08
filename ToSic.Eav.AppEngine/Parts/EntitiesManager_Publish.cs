namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        private void PublishWithoutPurge(int entityId)
        {
            Log.Add($"PublishWithoutPurge({entityId})");

            // 1. make sure we're publishing the draft, because the entityId might be the published one...
            var contEntity = AppManager.Read.Entities.Get(entityId);
            if (contEntity == null)
                Log.Add($"Will skip, couldn't find the entity {entityId}");
            else
            {
                Log.Add($"found id: {contEntity.EntityId}, " +
                        $"rid: {contEntity.RepositoryId}, isPublished: {contEntity.IsPublished}");

                var maybeDraft = contEntity.IsPublished
                    ? contEntity.GetDraft() ?? contEntity   // if no draft exists, use current
                    : contEntity;// if it isn't published, use current

                var repoId = maybeDraft.RepositoryId;

                Log.Add($"publish requested for:{entityId}, " +
                        $"will publish: {repoId} if published false (it's: {maybeDraft.IsPublished})");

                if (!maybeDraft.IsPublished)
                    AppManager.DataController.Publishing.PublishDraftInDbEntity(repoId, maybeDraft);
            }

            Log.Add($"/PublishWithoutPurge({entityId})");
        }

        /// <summary>
        /// Publish many entities
        /// </summary>
        public void Publish(int[] entityIds)
        {
            Log.Add(() => "Publish(" + entityIds.Length + " items [" + string.Join(",", entityIds) + "])");
            foreach (var eid in entityIds)
                try
                {
                    PublishWithoutPurge(eid);
                }
                catch (Repository.Efc.Parts.EntityAlreadyPublishedException) { /* ignore */ }
            // for now, do a full purge as it's the safest. in the future, maybe do a partial cache-update
            SystemManager.Purge(AppManager.AppId);
            Log.Add("/Publish(...)");
        }

        /// <summary>
        /// Publish a single entity 
        /// </summary>
        public void Publish(int entityId) => Publish(new[] { entityId });


    }
}

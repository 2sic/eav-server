using ToSic.Eav.Apps.AppSys;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        private void PublishWithoutPurge(IAppWorkCtx appCtx, int entityId) => Log.Do(l =>
        {
            l.A($"PublishWithoutPurge({entityId})");

            // 1. make sure we're publishing the draft, because the entityId might be the published one...
            var contEntity = _appWork.Entities.Get(appCtx, entityId);
            if (contEntity == null)
                l.A($"Will skip, couldn't find the entity {entityId}");
            else
            {
                l.A($"found id: {contEntity.EntityId}, " +
                      $"rid: {contEntity.RepositoryId}, isPublished: {contEntity.IsPublished}");

                var maybeDraft = contEntity.IsPublished
                    ? appCtx.AppState.GetDraft(contEntity) ?? contEntity // if no draft exists, use current
                    : contEntity; // if it isn't published, use current

                var repoId = maybeDraft.RepositoryId;

                l.A($"publish requested for:{entityId}, " +
                      $"will publish: {repoId} if published false (it's: {maybeDraft.IsPublished})");

                if (!maybeDraft.IsPublished)
                    Parent.DataController.Publishing.PublishDraftInDbEntity(repoId, maybeDraft);
            }

            l.A($"/PublishWithoutPurge({entityId})");
        });

        /// <summary>
        /// Publish many entities
        /// </summary>
        public void Publish(int[] entityIds)
        {
            Log.A(Log.Try(() => "Publish(" + entityIds.Length + " items [" + string.Join(",", entityIds) + "])"));
            var appCtx = Parent.AppState.CreateAppWorkCtx();
            foreach (var eid in entityIds)
                try
                {
                    PublishWithoutPurge(appCtx, eid);
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

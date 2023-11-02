﻿using ToSic.Lib.Logging;
using ToSic.Eav.Apps.AppSys;
using ToSic.Eav.Caching;
using ToSic.Lib.DI;

namespace ToSic.Eav.Apps.Work
{
    public class WorkEntityPublish : WorkUnitBase<IAppWorkCtxWithDb>
    {
        private readonly LazySvc<IAppLoaderTools> _appLoaderTools;
        private readonly AppsCacheSwitch _appsCache;
        private readonly AppWork _appWork;

        public WorkEntityPublish(AppWork appWork, LazySvc<IAppLoaderTools> appLoaderTools, AppsCacheSwitch appsCache /* Note: Singleton */) : base("AWk.EntPub")
        {
            ConnectServices(
                _appWork = appWork,
                _appLoaderTools = appLoaderTools,
                _appsCache = appsCache
            );
        }

        /// <summary>
        /// Publish a single entity 
        /// </summary>
        public void Publish(int entityId) => Publish(new[] { entityId });


        /// <summary>
        /// Publish many entities
        /// </summary>
        public void Publish(int[] entityIds)
        {
            var l = Log.Fn(Log.Try(() => "Publish(" + entityIds.Length + " items [" + string.Join(",", entityIds) + "])"));
            foreach (var eid in entityIds)
                try
                {
                    PublishWithoutPurge(eid);
                }
                catch (Repository.Efc.Parts.EntityAlreadyPublishedException) { /* ignore */ }
            // Tell the cache to do a partial update
            _appsCache.Value.Update(AppWorkCtx, entityIds, Log, _appLoaderTools.Value);
            l.Done();
        }


        private void PublishWithoutPurge(int entityId)
        {
            var l = Log.Fn($"PublishWithoutPurge({entityId})");

            // 1. make sure we're publishing the draft, because the entityId might be the published one...
            var contEntity = _appWork.Entities.Get(AppWorkCtx, entityId);
            if (contEntity == null)
                l.A($"Will skip, couldn't find the entity {entityId}");
            else
            {
                l.A($"found id: {contEntity.EntityId}, " +
                    $"rid: {contEntity.RepositoryId}, isPublished: {contEntity.IsPublished}");

                var maybeDraft = contEntity.IsPublished
                    ? AppWorkCtx.AppState.GetDraft(contEntity) ?? contEntity // if no draft exists, use current
                    : contEntity; // if it isn't published, use current

                var repoId = maybeDraft.RepositoryId;

                l.A($"publish requested for:{entityId}, " +
                    $"will publish: {repoId} if published false (it's: {maybeDraft.IsPublished})");

                if (!maybeDraft.IsPublished)
                    AppWorkCtx.DataController.Publishing.PublishDraftInDbEntity(repoId, maybeDraft);
            }

            l.Done($"/PublishWithoutPurge({entityId})");
        }
    }
}
using ToSic.Eav.Caching;
using ToSic.Eav.Data.Entities.Sys.Lists;

namespace ToSic.Eav.Apps.Internal.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkEntityPublish(AppsCacheSwitch appsCache)
    : WorkUnitBase<IAppWorkCtxWithDb>("AWk.EntPub", connect: [appsCache])
{

    /// <summary>
    /// Publish a single entity 
    /// </summary>
    public void Publish(int entityId) => Publish([entityId]);


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
        appsCache.Update(AppWorkCtx, entityIds);
        l.Done();
    }


    private void PublishWithoutPurge(int entityId)
    {
        var l = Log.Fn($"PublishWithoutPurge({entityId})");

        // 1. make sure we're publishing the draft, because the entityId might be the published one...
        var contEntity = AppWorkCtx.AppReader.List.FindRepoId(entityId);
        if (contEntity == null)
            l.A($"Will skip, couldn't find the entity {entityId}");
        else
        {
            l.A($"found id: {contEntity.EntityId}, " +
                $"rid: {contEntity.RepositoryId}, isPublished: {contEntity.IsPublished}");

            var maybeDraft = contEntity.IsPublished
                ? AppWorkCtx.AppReader.GetDraft(contEntity) ?? contEntity // if no draft exists, use current
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
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Repository.Efc.Sys.DbParts;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkEntityPublish(AppsCacheSwitch appsCache)
    : ServiceBase("AWk.EntPub", connect: [appsCache])
{
    /// <summary>
    /// Publish many entities
    /// </summary>
    public void Publish(IAppWorkCtxForDiWip appWorkCtx, int[] entityIds)
    {
        var l = Log.Fn(Log.Try(() => $"Publish({entityIds.Length} items [{string.Join(",", entityIds)}])"));
        foreach (var eid in entityIds)
            try
            {
                PublishWithoutPurge(appWorkCtx, eid);
            }
            catch (EntityAlreadyPublishedException)
            {
                 /* ignore */
            }
        // Tell the cache to do a partial update
        appsCache.Update(appWorkCtx, entityIds);
        l.Done();
    }


    private bool PublishWithoutPurge(IAppWorkCtxForDiWip appWorkCtx, int entityId)
    {
        var l = Log.Fn<bool>($"{entityId}");

        // 1. make sure we're publishing the draft, because the entityId might be the published one...
        var contEntity = appWorkCtx.AppReader.List.FindRepoId(entityId);
        
        // Exit early
        if (contEntity == null)
            return l.ReturnFalse($"Will skip, couldn't find the entity {entityId}");
        
        l.A($"found id: {contEntity.EntityId}, " +
            $"rid: {contEntity.RepositoryId}, isPublished: {contEntity.IsPublished}");

        var entityMaybeDraft = contEntity.IsPublished
            ? appWorkCtx.AppReader.GetDraft(contEntity) ?? contEntity // if no draft exists, use current
            : contEntity; // if it isn't published, use current

        var repoId = entityMaybeDraft.RepositoryId;

        l.A($"publish requested for:{entityId}, " +
            $"will publish: {repoId} if published false (it's: {entityMaybeDraft.IsPublished})");

        if (entityMaybeDraft.IsPublished)
            return l.ReturnFalse("already published");
        
        // implement final changes
        appWorkCtx.DbStorage.Publishing.PublishDraftInDbEntity(repoId, entityMaybeDraft);
        return l.ReturnTrue("published");

    }
}
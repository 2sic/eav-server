using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps.State;

partial class AppState
{

    #region GetDraft and GetPublished

    /// <summary>
    /// If entity is published and there is a draft of it, then it can be navigated through DraftEntity
    /// </summary>
    [PrivateApi]
    internal IEntity GetDraft(IEntity entity)
    {
        if (entity == null) return null;
        if (!entity.IsPublished) return null;
        // 2023-03-28 2dm - I don't think the RepoId is correct here, the publish item still has it's original EntityId...?
        var publishedEntityId = entity.EntityId; // ((Entity) entity).RepositoryId;
        // Try to get it, but make sure we only return it if it has a different repo-id - very important
        if (ListDrafts.Value.TryGetValue(publishedEntityId, out var result) && result.RepositoryId == publishedEntityId)
            return null;
        return result;
        //return Index.Values.FirstOrDefault(draftEntity => draftEntity.IsPublished == false && draftEntity.EntityId == publishedEntityId);
    }

    /// <summary>
    /// If entity is draft and there is a published edition, then it can be navigated through PublishedEntity
    /// </summary>
    [PrivateApi]
    internal IEntity GetPublished(IEntity entity)
    {
        if (entity == null || entity.IsPublished) return null;
        return Index.TryGetValue(entity.EntityId, out var pub) && pub.IsPublished ? pub : null;
    }

    #endregion


    /// <summary>
    /// Get all Published Entities in this App (excluding Drafts)
    /// </summary>
    [PrivateApi("this is an optimization feature which shouldn't be used by others")]
    internal SynchronizedList<IEntity> ListPublished => _listPublished
        ??= new SynchronizedEntityList(this, () => List.Where(e => e.IsPublished).ToImmutableList());

    private SynchronizedEntityList _listPublished;

    /// <summary>
    /// Get all Entities not having a Draft (Entities that are Published (not having a draft) or draft itself)
    /// </summary>
    [PrivateApi("this is an optimization feature which shouldn't be used by others")]
    internal SynchronizedList<IEntity> ListNotHavingDrafts => _listNotHavingDrafts 
        ??= new SynchronizedEntityList(this, () => List.Where(e => GetDraft(e) == null).ToImmutableList());

    private SynchronizedEntityList _listNotHavingDrafts;


    /// <summary>
    /// Get all Draft Entities in this App
    /// </summary>
    [PrivateApi("this is an optimization feature which shouldn't be used by others")]
    private SynchronizedObject<ImmutableDictionary<int, IEntity>> ListDrafts => _listDrafts
        ??= new SynchronizedObject<ImmutableDictionary<int, IEntity>>(this, () =>
        {
            var unpublished = List.Where(e => e.IsPublished == false);
            // there are rare cases where the main item is unpublished and it also has a draft which points to it
            // this would result in duplicate entries in the index, so we have to be very sure we don't have these
            // so we deduplicate and keep the last - otherwise we would break this dictionary.
            var lastOnly = unpublished.GroupBy(e => e.EntityId).Select(g => g.Last()).ToList();
            return lastOnly.ToImmutableDictionary(e => e.EntityId);
        });

    private SynchronizedObject<ImmutableDictionary<int, IEntity>> _listDrafts;
    
}
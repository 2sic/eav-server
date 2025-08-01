using System.Collections.Immutable;
using ToSic.Eav.Data.Entities.Sys;
using ToSic.Sys.Caching.Synchronized;

namespace ToSic.Eav.Apps.Sys.State;
internal class AppStatePublishing(AppState appState)
{
    #region Lists Published etc.

    /// <summary>
    /// Get all Published Entities in this App (excluding Drafts)
    /// </summary>
    [PrivateApi("this is an optimization feature which shouldn't be used by others")]
    internal SynchronizedList<IEntity> ListPublished => _listPublished
        ??= new(appState, () => appState.Entities.ImmutableList.Where(e => e.IsPublished).ToImmutableOpt());

    private SynchronizedEntityList? _listPublished;

    /// <summary>
    /// Get all Entities not having a Draft (Entities that are Published (not having a draft) or draft itself)
    /// </summary>
    [PrivateApi("this is an optimization feature which shouldn't be used by others")]
    internal SynchronizedList<IEntity> ListNotHavingDrafts => _listNotHavingDrafts
        ??= new(appState, () => appState.Entities.ImmutableList.Where(e => GetDraft(e) == null).ToImmutableOpt());

    private SynchronizedEntityList? _listNotHavingDrafts;


    #endregion


    #region Draft / Published

    /// <summary>
    /// If entity is draft and there is a published edition, then it can be navigated through PublishedEntity
    /// </summary>
    [PrivateApi]
    internal IEntity? GetPublished(IEntity? entity)
    {
        if (entity == null || entity.IsPublished)
            return null;
        return appState.Entities.Index.TryGetValue(entity.EntityId, out var pub) && pub.IsPublished
            ? pub
            : null;
    }

    /// <summary>
    /// If entity is published and there is a draft of it, then it can be navigated through DraftEntity
    /// </summary>
    [PrivateApi]
    internal IEntity? GetDraft(IEntity? entity)
    {
        if (entity == null)
            return null;
        if (!entity.IsPublished)
            return null;
        var publishedEntityId = entity.EntityId;
        // Try to get it, but make sure we only return it if it has a different repo-id - very important
        if (ListDrafts.Value.TryGetValue(publishedEntityId, out var result) && result.RepositoryId == publishedEntityId)
            return null;
        return result;
    }


    /// <summary>
    /// Get all Draft Entities in this App
    /// </summary>
    [PrivateApi("this is an optimization feature which shouldn't be used by others")]
    [field: AllowNull, MaybeNull]
    private SynchronizedObject<ImmutableDictionary<int, IEntity>> ListDrafts => field
        ??= new(appState, () =>
        {
            var unpublished = appState.Entities.ImmutableList.Where(e => e.IsPublished == false);
            // there are rare cases where the main item is unpublished, and it also has a draft which points to it
            // this would result in duplicate entries in the index, so we have to be very sure we don't have these
            // so we deduplicate and keep the last - otherwise we would break this dictionary.
            var lastOnly = unpublished
                .GroupBy(e => e.EntityId)
                .Select(g => g.Last())
                .ToListOpt();
            return lastOnly.ToImmutableDictionary(e => e.EntityId);
        });


    #endregion

}

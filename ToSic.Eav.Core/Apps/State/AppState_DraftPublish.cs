using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps
{
    public partial class AppState
    {

        #region GetDraft and GetPublished

        /// <summary>
        /// If entity is published and there is a draft of it, then it can be navigated through DraftEntity
        /// </summary>
        [PrivateApi]
        public IEntity GetDraft(IEntity entity)
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
        public IEntity GetPublished(IEntity entity)
        {
            if (entity == null) return null;
            if (entity.IsPublished) return null;
            var publishedEntityId = ((Entity)entity).EntityId;
            return Index.ContainsKey(publishedEntityId) ? Index[publishedEntityId] : null;
        }

        #endregion


        /// <summary>
        /// Get all Published Entities in this App (excluding Drafts)
        /// </summary>
        [PrivateApi("this is an optimization feature which shouldn't be used by others")]
        public SynchronizedList<IEntity> ListPublished
            => _listPublished ??= new SynchronizedEntityList(this,
                () => List.Where(e => e.IsPublished).ToImmutableList());

        private SynchronizedEntityList _listPublished;

        /// <summary>
        /// Get all Entities not having a Draft (Entities that are Published (not having a draft) or draft itself)
        /// </summary>
        [PrivateApi("this is an optimization feature which shouldn't be used by others")]
        public SynchronizedList<IEntity> ListNotHavingDrafts
            => _listNotHavingDrafts ??= new SynchronizedEntityList(this,
                () => List.Where(e => GetDraft(e) == null).ToImmutableList());

        private SynchronizedEntityList _listNotHavingDrafts;


        /// <summary>
        /// Get all Draft Entities in this App
        /// </summary>
        [PrivateApi("this is an optimization feature which shouldn't be used by others")]
        private SynchronizedObject<ImmutableDictionary<int, IEntity>> ListDrafts
            => _listDrafts ??= new SynchronizedObject<ImmutableDictionary<int, IEntity>>(this,
                () =>
                {
                    var unpublished = List.Where(e => e.IsPublished == false);
                    // there are rare cases where the main item is unpublished and it also has a draft which points to it
                    // this would result in duplicate entries in the index, so we have to be very sure we don't have these
                    // so we deduplicate and keep the last - otherwise we would break this dictionary.
                    var lastOnly = unpublished.GroupBy(e => e.EntityId).Select(g => g.Last()).ToList();
                    return lastOnly.ToImmutableDictionary(e => e.EntityId);
                });

        private SynchronizedObject<ImmutableDictionary<int, IEntity>> _listDrafts;

        /// <summary>
        /// Reconnect / wire drafts to the published item
        /// </summary>
        private void MapDraftToPublished(Entity newEntity, int? publishedId, bool log)
        {
            // fix: #3070, publishedId sometimes has value 0, but that one should not be used
            if (newEntity.IsPublished || !publishedId.HasValue || publishedId.Value == 0) return;

            if (log) Log.A($"map draft to published for new: {newEntity.EntityId} on {publishedId}");

            // Published Entity is already in the Entities-List as EntityIds is validated/extended before and Draft-EntityID is always higher as Published EntityId
            //newEntity.PublishedEntity = Index[publishedId.Value];
            //((Entity)newEntity.PublishedEntity).DraftEntity = newEntity;
            newEntity.EntityId = publishedId.Value; // this is not immutable, but probably not an issue because it is not in the index yet
        }

        /// <summary>
        /// Check if a new entity previously had a draft, and remove that
        /// </summary>
        /// <param name="newEntity"></param>
        /// <param name="log">To optionally disable logging, in case it would overfill what we're seeing!</param>
        private void RemoveObsoleteDraft(IEntity newEntity, bool log)
        {
            var previous = Index.ContainsKey(newEntity.EntityId) ? Index[newEntity.EntityId] : null;
            var draftEnt = GetDraft(previous);

            // check if we went from draft-branch to published, because in this case, we have to remove the last draft
            string msg = null;
            if (previous == null) msg = "previous is null => new will be added to cache";  // didn't exist, return
            else if (!previous.IsPublished) msg = "previous not published => new will replace in cache"; // previous wasn't published, so we couldn't have had a branch
            else if (!newEntity.IsPublished && draftEnt == null) msg = "new copy not published, and no draft exists => new will replace in cache"; // new entity isn't published, so we're not switching "back"

            if (msg != null)
            {
                if (log) Log.A("remove obsolete draft - nothing to change because: " + msg);
                return;
            }

            var draftId = draftEnt?.RepositoryId;
            if (draftId != null)
            {
                if (log) Log.A($"remove obsolete draft - found draft, will remove {draftId.Value}");
                Index.Remove(draftId.Value);
            }
            else
                if (log) Log.A("remove obsolete draft - no draft, won't remove");
        }

    }
}

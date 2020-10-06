using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    public partial class AppState
    {
        /// <summary>
        /// Get all Published Entities in this App (excluding Drafts)
        /// </summary>
        [PrivateApi("this is an optimization feature which shouldn't be used by others")]
        public SynchronizedList<IEntity> ListPublished
            => _listPublished ?? (_listPublished = new SynchronizedList<IEntity>(this,
                   () => List.Where(e => e.IsPublished).ToImmutableArray())); //.ToList()));
        private SynchronizedList<IEntity> _listPublished;

        /// <summary>
        /// Get all Entities not having a Draft (Entities that are Published (not having a draft) or draft itself)
        /// </summary>
        [PrivateApi("this is an optimization feature which shouldn't be used by others")]
        public SynchronizedList<IEntity> ListNotHavingDrafts
            => _listNotHavingDrafts ?? (_listNotHavingDrafts =
                   new SynchronizedList<IEntity>(this,
                       () => List.Where(e => e.GetDraft() == null).ToImmutableArray())); //.ToList()));
        private SynchronizedList<IEntity> _listNotHavingDrafts;


        /// <summary>
        /// Reconnect / wire drafts to the published item
        /// </summary>
        private void MapDraftToPublished(Entity newEntity, int? publishedId, bool log)
        {
            if (newEntity.IsPublished || !publishedId.HasValue) return;

            if (log) Log.Add($"map draft to published for new: {newEntity.EntityId} on {publishedId}");

            // Published Entity is already in the Entities-List as EntityIds is validated/extended before and Draft-EntityID is always higher as Published EntityId
            newEntity.PublishedEntity = Index[publishedId.Value];
            ((Entity)newEntity.PublishedEntity).DraftEntity = newEntity;
            newEntity.EntityId = publishedId.Value;
        }

        /// <summary>
        /// Check if a new entity previously had a draft, and remove that
        /// </summary>
        /// <param name="newEntity"></param>
        /// <param name="log">To optionally disable logging, in case it would overfill what we're seeing!</param>
        private void RemoveObsoleteDraft(IEntity newEntity, bool log)
        {
            var previous = Index.ContainsKey(newEntity.EntityId) ? Index[newEntity.EntityId] : null;
            var draftEnt = previous?.GetDraft();

            // check if we went from draft-branch to published, because in this case, we have to remove the last draft
            string msg = null;
            if (previous == null) msg = "previous is null => new will be added to cache";  // didn't exist, return
            else if (!previous.IsPublished) msg = "previous not published => new will replace in cache"; // previous wasn't published, so we couldn't have had a branch
            else if (!newEntity.IsPublished && draftEnt == null) msg = "new copy not published, and no draft exists => new will replace in cache"; // new entity isn't published, so we're not switching "back"

            if (msg != null)
            {
                if (log) Log.Add("remove obsolete draft - nothing to change because: " + msg);
                return;
            }

            var draftId = draftEnt?.RepositoryId;
            if (draftId != null)
            {
                if (log) Log.Add($"remove obsolete draft - found draft, will remove {draftId.Value}");
                Index.Remove(draftId.Value);
            }
            else
                if (log) Log.Add("remove obsolete draft - no draft, won't remove");
        }

    }
}

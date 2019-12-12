using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// A complete App state - usually cached in memory. <br/>
    /// Has many internal features for partial updates etc.
    /// But the primary purpose is to make sure the whole app is always available with everything. <br/>
    /// It also manages and caches relationships between entities of the same app.
    /// </summary>
    [PublicApi]
    public partial class AppState: HasLog, IAppIdentityLight
	{

		#region public properties like AppId, Entities, List, Publisheentities, DraftEntities, 
        /// <inheritdoc />
        public int AppId { get; }

	    /// <summary>
	    /// The simple list of <em>all</em> entities, used everywhere
	    /// </summary>
	    public IEnumerable<IEntity> List => _list 
            ?? (_list = new SynchronizedList<IEntity>(this, () => Index.Values.ToList()));
	    private SynchronizedList<IEntity> _list;

        internal Dictionary<int, IEntity> Index { get; }

	    /// <summary>
	    /// Get all Published Entities in this App (excluding Drafts)
	    /// </summary>
	    [PrivateApi("this is an optimization feature which shouldn't be used by others")]
	    public IEnumerable<IEntity> ListPublished
	        => _listPublished ?? (_listPublished = new SynchronizedList<IEntity>(this,
	               () => List.Where(e => e.IsPublished).ToList()));
	    private IEnumerable<IEntity> _listPublished;

	    /// <summary>
	    /// Get all Entities not having a Draft (Entities that are Published (not having a draft) or draft itself)
	    /// </summary>
	    [PrivateApi("this is an optimization feature which shouldn't be used by others")]
	    public IEnumerable<IEntity> ListNotHavingDrafts
	        => _listNotHavingDrafts ?? (_listNotHavingDrafts =
	               new SynchronizedList<IEntity>(this,
	                   () => List.Where(e => e.GetDraft() == null).ToList()));
	    private IEnumerable<IEntity> _listNotHavingDrafts;

        /// <summary>
        /// Manages all relationships between Entities
        /// </summary>
        public AppRelationshipManager Relationships { get; }

	    private bool _loading;
	    private bool _firstLoadCompleted;
        [PrivateApi]
	    public int DynamicUpdatesCount;
		#endregion


        [PrivateApi("constructor, internal use only")]
        internal AppState(int appId, ILog parentLog): base($"App.Pkg{appId}", parentLog, $"start build package for {appId}")
	    {
	        AppId = appId;
            CacheResetTimestamp();  // do this very early, as this number is needed elsewhere

	        Index = new Dictionary<int, IEntity>();
            Relationships = new AppRelationshipManager(this);
	        History.Add("app-data-cache", Log);
        }

        /// <summary>
        /// The first init-command to run after creating the package
        /// it's needed, so the metadata knows what lookup types are supported
        /// </summary>
        /// <param name="metadataTypes"></param>
        [PrivateApi("internal use only")]
        internal void InitMetadata(ImmutableDictionary<int, string> metadataTypes)
	    {
            if(!_loading)
                throw new Exception("trying to init metadata, but not in loading state. set that first!");
	        Metadata = _appTypesFromRepository == null
	            ? new AppMetadataManager(this, metadataTypes, Log)
	            : throw new Exception("can't set metadata if content-types are already set");
	    }


	    /// <summary>
        /// Add an entity to the cache. Should only be used by EAV code
        /// </summary>
	    internal void Add(Entity newEntity, int? publishedId)
	    {
	        if (!_loading)
	            throw new Exception("trying to add entity, but not in loading state. set that first!");

            if (newEntity.RepositoryId == 0)
                throw new Exception("Entities without real ID not supported yet");

            CacheResetTimestamp(); 
	        RemoveObsoleteDraft(newEntity);
            Index[newEntity.RepositoryId] = newEntity; // add like this, it could also be an update
	        MapDraftToPublished(newEntity, publishedId);
            Metadata.Register(newEntity);

	        if (_firstLoadCompleted)
	            DynamicUpdatesCount++;

	        Log.Add($"added entity {newEntity.EntityId} for published {publishedId}; dyn-update#{DynamicUpdatesCount}");
	    }

        /// <summary>
        /// Reset all item storages and indexes
        /// </summary>
	    internal void RemoveAllItems()
        {
            Log.Add("remove all items");
	        Index.Clear();
            CacheResetTimestamp(); 
            Metadata.Reset();
	    }


        /// <summary>
        /// Reconnect / wire drafts to the published item
        /// </summary>
	    private void MapDraftToPublished(Entity newEntity, int? publishedId)
        {
	        if (newEntity.IsPublished || !publishedId.HasValue) return;

            Log.Add($"map draft to published for new: {newEntity.EntityId} on {publishedId}");
	
            // Published Entity is already in the Entities-List as EntityIds is validated/extended before and Draft-EntityID is always higher as Published EntityId
            newEntity.PublishedEntity = Index[publishedId.Value];
	        ((Entity) newEntity.PublishedEntity).DraftEntity = newEntity;
	        newEntity.EntityId = publishedId.Value;
	    }

	    /// <summary>
        /// Check if a new entity previously had a draft, and remove that
        /// </summary>
	    private void RemoveObsoleteDraft(IEntity newEntity)
	    {
	        var previous = Index.ContainsKey(newEntity.EntityId) ? Index[newEntity.EntityId] : null;
            var draftEnt = previous?.GetDraft();

            // check if we went from draft-branch to published, because in this case, we have to remove the last draft
            string msg = null;
	        if (previous == null) msg = "previous is null => new will be added to cache";  // didn't exist, return
            else if(!previous.IsPublished) msg = "previous not published => new will replace in cache"; // previous wasn't published, so we couldn't have had a branch
	        else if(!newEntity.IsPublished && draftEnt == null) msg = "new copy not published, and no draft exists => new will replace in cache"; // new entity isn't published, so we're not switching "back"

	        if (msg != null)
	        {
	            Log.Add("remove obsolete draft - nothing to change because: " + msg);
                return;
	        }

            var draftId = draftEnt?.RepositoryId;
	        if (draftId != null)
	        {
	            Log.Add($"remove obsolete draft - found draft, will remove {draftId.Value}");
	            Index.Remove(draftId.Value);
	        }
	        else
	            Log.Add("remove obsolete draft - no draft, won't remove");
	    }


	    internal void Load(ILog parentLog, Action loader)
	    {
	        _loading = true;
            Log.LinkTo(parentLog);
	        Log.Add("app loading start");
	        loader.Invoke();
	        _loading = false;
	        _firstLoadCompleted = true;
	        Log.Add($"app loading done - dynamic load count: {DynamicUpdatesCount}");
	        Log.LinkTo(null);
        }

    }
}
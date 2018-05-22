using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.App
{
    /// <inheritdoc cref="HasLog" />
    /// <summary>
    /// Cache Object for a specific App
    /// </summary>
    public partial class AppDataPackage: HasLog
	{

		#region public properties like AppId, Entities, List, Publisheentities, DraftEntities, 
        /// <summary>
        /// App ID
        /// </summary>
        public int AppId { get; }

	    /// <summary>
	    /// The simple list of entities, used in many query parts
	    /// </summary>
	    public IEnumerable<IEntity> List => _list 
            ?? (_list = new CacheChainedIEnumerable<IEntity>(this, () => Index.Values.ToList()));
	    private CacheChainedIEnumerable<IEntity> _list;

        internal Dictionary<int, IEntity> Index { get; }

	    /// <summary>
	    /// Get all Published Entities in this App (excluding Drafts)
	    /// </summary>
	    public IEnumerable<IEntity> ListPublished
	        => _listPublished ?? (_listPublished = new CacheChainedIEnumerable<IEntity>(this,
	               () => List.Where(e => e.IsPublished).ToList()));
	    private IEnumerable<IEntity> _listPublished;

	    /// <summary>
	    /// Get all Entities not having a Draft (Entities that are Published (not having a draft) or draft itself)
	    /// </summary>
	    public IEnumerable<IEntity> ListNotHavingDrafts
	        => _listNotHavingDrafts ?? (_listNotHavingDrafts =
	               new CacheChainedIEnumerable<IEntity>(this,
	                   () => List.Where(e => e.GetDraft() == null).ToList()));
	    private IEnumerable<IEntity> _listNotHavingDrafts;

	    ///// <summary>
	    ///// Get all Entities not having a Draft (Entities that are Published (not having a draft) or draft itself)
	    ///// </summary>
	    //public IEnumerable<IEntity> ListDraft
	    //    => _listDraft ?? (_listDraft = new UpstreamDependentIEnumerable<IEntity>(this,
	    //           () => List.Where(e => !e.IsPublished).ToList()));
	    //private IEnumerable<IEntity> _listDraft;

        /// <summary>
        /// Get all Relationships between Entities
        /// </summary>
        public AppRelationshipManager Relationships { get; }

	    private bool _loading;
	    private bool _firstLoadCompleted;
	    public int DynamicUpdatesCount;
		#endregion



        internal AppDataPackage(int appId, Log parentLog): base($"App.Pkg{appId}", parentLog, $"start build package for {appId}")
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
        internal void InitMetadata(ImmutableDictionary<int, string> metadataTypes)
	    {
            if(!_loading)
                throw new Exception("trying to init metadata, but not in loading state. set that first!");
	        Metadata = _appTypesFromRepository == null
	            ? new AppMetadataManager(this, metadataTypes, Log)
	            : throw new Exception("can't set metadata if content-types are already set");
	    }


	    /// <summary>
        /// Add an entity to the cache
        /// </summary>
	    public void Add(Entity newEntity, int? publishedId)
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

	        // check if we went from draft-branch to published, because in this case, we have to remove the last draft
	        string msg = null;
	        if (previous == null) msg = "previous == null";  // didn't exist, return
            else if(!previous.IsPublished) msg = "previous not published"; // previous wasn't published, so we couldn't have had a branch
	        else if(!newEntity.IsPublished) msg = "new not published"; // new entity isn't published, so we're not switching "back"

	        if (msg != null)
	        {
	            Log.Add("remove obsolete draft - nothing to change " + msg);
                return;
	        }

            var draftEnt = previous.GetDraft();
            var draft = draftEnt?.RepositoryId;
	        if (draft != null)
	        {
	            Log.Add($"remove obsolete draft - found draft, will remove {draft.Value}");
	            Index.Remove(draft.Value);
	        }
	        else
	            Log.Add("remove obsolete draft - no draft, won't remove");
	    }

	    public void Load(Log parentLog, Action loader)
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
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
        /// The simple list of entities, used in many pipeline parts
        /// </summary>
        public IEnumerable<IEntity> List { get; }

        internal Dictionary<int, IEntity> Index { get; }

		/// <summary>
		/// Get all Published Entities in this App (excluding Drafts)
		/// </summary>
		//public IEnumerable<IEntity> PublishedEntities => _publishedEntities 
  //          ?? (_publishedEntities = List.Where(e => e.IsPublished));
	 //   private IEnumerable<IEntity> _publishedEntities;

        public IEnumerable<IEntity> PublishedEntities => new AppDependentIEnumerable<IEntity>(this, 
            () => List.Where(e => e.IsPublished).ToList());

        /// <summary>
        /// Get all Entities not having a Draft (Entities that are Published (not having a draft) or draft itself)
        /// </summary>
        //   public IEnumerable<IEntity> DraftEntities => _draftEntities 
        //       ?? (_draftEntities = List.Where(e => e.GetDraft() == null));
        //private IEnumerable<IEntity> _draftEntities;
        public IEnumerable<IEntity> DraftEntities => new AppDependentIEnumerable<IEntity>(this, 
            () => List.Where(e => e.GetDraft() == null).ToList());

        /// <summary>
        /// Get all Relationships between Entities
        /// </summary>
        public AppRelationshipManager Relationships { get; }

	    public int DynamicUpdatesCount = 0;
		#endregion



        internal AppDataPackage(int appId, Log parentLog): base("App.Packge", parentLog)
	    {
	        AppId = appId;
            CacheResetTimestamp();  // do this very early, as this number is needed elsewhere

	        Index = new Dictionary<int, IEntity>();
	        List = Index.Values;
            Relationships = new AppRelationshipManager(this);

        }

        /// <summary>
        /// The first init-command to run after creating the package
        /// it's needed, so the metadata knows what lookup types are supported
        /// </summary>
        /// <param name="metadataTypes"></param>
	    internal void InitMetadata(ImmutableDictionary<int, string> metadataTypes)
	        => Metadata = _appTypesFromRepository == null
	            ? new AppMetadataManager(this, metadataTypes)
	            : throw new Exception("can't set metadata if content-types are already set");


        /// <summary>
        /// Add an entity to the cache
        /// </summary>
	    public void Add(Entity newEntity, int? publishedId)
	    {
            if(newEntity.RepositoryId == 0)
                throw new Exception("Entities without real ID not supported yet");

            CacheResetTimestamp(); // for relationships
	        RemoveObsoleteDraft(newEntity);
            Index[newEntity.RepositoryId] = newEntity; // add like this, it could also be an update
	        MapDraftToPublished(newEntity, publishedId);
            Metadata.Register(newEntity);
	    }

        /// <summary>
        /// Reset all item storages and indexes
        /// </summary>
	    internal void RemoveAllItems()
	    {
	        Index.Clear();
            CacheResetTimestamp(); // for relationships
            Metadata.Reset();
	    }


        /// <summary>
        /// Reconnect / wire drafts to the published item
        /// </summary>
	    private void MapDraftToPublished(Entity newEntity, int? publishedId)
	    {
	        if (newEntity.IsPublished || !publishedId.HasValue) return;

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
	        if (previous == null) return;  // didn't exist, return
            if(!previous.IsPublished) return; // previous wasn't published, so we couldn't have had a branch
	        if(!newEntity.IsPublished) return; // new entity isn't published, so we're not switching "back"

	        var draftEnt = previous.GetDraft();
            var draft = draftEnt?.RepositoryId;
	        if (draft != null)
	            Index.Remove(draft.Value);
	    }
	}
}
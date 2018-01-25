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
		public IEnumerable<IEntity> PublishedEntities => _publishedEntities 
            ?? (_publishedEntities = List.Where(e => e.IsPublished));
	    private IEnumerable<IEntity> _publishedEntities;

        /// <summary>
        /// Get all Entities not having a Draft (Entities that are Published (not having a draft) or draft itself)
        /// </summary>
        public IEnumerable<IEntity> DraftEntities => _draftEntities 
            ?? (_draftEntities = List.Where(e => e.GetDraft() == null));

	    private IEnumerable<IEntity> _draftEntities;


        /// <summary>
        /// Get all Relationships between Entities
        /// </summary>
        public AppRelationshipManager Relationships { get; }

        /// <summary>
        /// Gets the DateTime when this CacheItem was populated
        /// </summary>
        public DateTime LastRefresh { get; }

	    public int DynamicUpdatesCount = 0;
		#endregion



        internal AppDataPackage(int appId, Log parentLog): base("App.Packge", parentLog)
	    {
	        AppId = appId;

	        Index = new Dictionary<int, IEntity>();
	        List = Index.Values;
            Relationships = new AppRelationshipManager(this);

            LastRefresh = DateTime.Now;
	    }

        /// <summary>
        /// The first init-command to run after creating the package
        /// it's needed, so the metadata knows what lookup types are supported
        /// </summary>
        /// <param name="metadataTypes"></param>
	    internal void InitMetadata(ImmutableDictionary<int, string> metadataTypes)
	        => Metadata = _appTypesFromRepository == null
	            ? new AppMetadataManager(metadataTypes)
	            : throw new Exception("can't set metadata if content-types are already set");


        /// <summary>
        /// The second init-command
        /// Load content-types
        /// </summary>
        /// <param name="contentTypes"></param>
	    internal void InitContentTypes(IList<IContentType> contentTypes)
	    {
	        if (Metadata == null || List.Any())
	            throw new Exception("can't set content types before setting Metadata manager, or after entities-list already exists");

	        _appTypeMap = contentTypes.ToImmutableDictionary(x => x.ContentTypeId, x => x.StaticName);
	        _appTypesFromRepository = RemoveAliasesForGlobalTypes(contentTypes);
	        // build types by name
	        BuildCacheForTypesByName(_appTypesFromRepository);
	    }


	    internal void Add(IEntity newEntity)
	    {
            if(newEntity.RepositoryId == 0)
                throw new Exception("Entities without real ID not supported yet");

            //if (Index.ContainsKey(newEntity.RepositoryId))
            //    throw new Exception("updating not supported yet");

            Metadata.Add((Entity)newEntity);

	        Index[newEntity.RepositoryId] = newEntity; // add like this, it could also be an update
            //Index.Add(newEntity.RepositoryId, newEntity);

            // Relationships uses the index, but it must know that it's now invalid
            Relationships.Reset(); 
	    }

        /// <summary>
        /// Reset all item storages and indexes
        /// </summary>
	    internal void ResetItems()
	    {
	        Index.Clear();
            Relationships.Reset();
            Metadata.Reset();
	    }

	    public void AddAndMapDraftToPublished(Entity newEntity, int? publishedId)
	    {
            // always add first - the rest is only if it's draft with published
            Add(newEntity);

	        if (newEntity.IsPublished || !publishedId.HasValue) return;

	        // Published Entity is already in the Entities-List as EntityIds is validated/extended before and Draft-EntityID is always higher as Published EntityId
	        newEntity.PublishedEntity = Index[publishedId.Value];
	        ((Entity)newEntity.PublishedEntity).DraftEntity = newEntity;
	        newEntity.EntityId = publishedId.Value;

	    }

    }
}
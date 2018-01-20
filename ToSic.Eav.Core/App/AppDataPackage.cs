using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
		#region private entities lists
		private IEnumerable<IEntity> _publishedEntities;
		private IEnumerable<IEntity> _draftEntities;
		#endregion

		#region public properties like AppId, Entities, List, Publisheentities, DraftEntities, 
        /// <summary>
        /// App ID
        /// </summary>
        public int AppId { get; }

        /// <summary>
        /// The simple list of entities, used in many pipeline parts
        /// </summary>
        public IEnumerable<IEntity> List { get; }// { get; private set; } 

        internal Dictionary<int, IEntity> Index { get; }

		/// <summary>
		/// Get all Published Entities in this App (excluding Drafts)
		/// </summary>
		public IEnumerable<IEntity> PublishedEntities => _publishedEntities ?? (_publishedEntities = List.Where(e => e.IsPublished));//.ToDictionary(k => k.Key, v => v.Value));

	    /// <summary>
		/// Get all Entities not having a Draft (Entities that are Published (not having a draft) or draft itself)
		/// </summary>
		public IEnumerable<IEntity> DraftEntities => _draftEntities ?? (_draftEntities = List.Where(e => e.GetDraft() == null));//.ToDictionary(k => k.Value.EntityId, v => v.Value));


        /// <summary>
        /// Get all Relationships between Entities
        /// </summary>
        public AppRelationshipManager Relationships { get; }

        /// <summary>
        /// Gets the DateTime when this CacheItem was populated
        /// </summary>
        public DateTime LastRefresh { get; }
		#endregion



        internal AppDataPackage(int appId, Log parentLog): base("App.Packge", parentLog)
	    {
	        AppId = appId;

	        Index = new Dictionary<int, IEntity>();
	        List = Index.Values;
            Relationships = new AppRelationshipManager(Index);

            // create a self-referencing deferred entities list
            var deferred = new AppDataPackageDeferredList();
	        deferred.AttachApp(this);
	        BetaDeferred = deferred;

            LastRefresh = DateTime.Now;
	    }

	    internal void Set1MetadataManager(AppMetadataManager metadataManager)
	    {
	        if (_appTypesFromRepository != null)
	            throw new Exception("can't set metadata if content-types are already set");
	        Metadata = metadataManager;
	    }


	    internal void Set2ContentTypes(IList<IContentType> contentTypes)
	    {
	        if (Metadata == null)
	            throw new Exception("can't set content types before setting Metadata manager");
	        if (List.Any())
	            throw new Exception("can't set content-types if entities List already exist");

	        _appTypeMap = contentTypes.ToImmutableList();
	        _appTypesFromRepository = RemoveAliasesForGlobalTypes(contentTypes);
	        // build types by name
	        BuildCacheForTypesByName(_appTypesFromRepository);
	    }


	    internal void Add(IEntity newEntity)
	    {
            if(newEntity.RepositoryId == 0)
                throw new Exception("Entities without real ID not supported yet");

            if (Index.ContainsKey(newEntity.RepositoryId))
                throw new Exception("updating not supported yet");

            Index.Add(newEntity.RepositoryId, newEntity);
	        //((List<IEntity>) List).Add(newEntity);
	    }

	    internal void Reset()
	    {
	        Index.Clear();
            Relationships.Clear();
	    }

    }
}
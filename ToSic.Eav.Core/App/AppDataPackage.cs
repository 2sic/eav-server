using System;
using System.Collections.Generic;
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
        public IEnumerable<IEntity> List { get; private set; } 

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

		/// <summary>
		/// Construct a new CacheItem with all required Items
		/// </summary>
		internal AppDataPackage(
            int appId,
            IEnumerable<IEntity> entList,
            IList<IContentType> contentTypes,
            AppMetadataManager metadataManager,
            //AppDataPackageDeferredList selfDeferredEntitiesList, 
            Log parentLog): this(appId, parentLog)// base("App.Packge", parentLog)
		{
		    //AppId = appId;

		    Set1MetadataManager(metadataManager);

		    Set2ContentTypes(contentTypes);

		    Set3Entities(entList);

			//Relationships = relationships;

			//LastRefresh = DateTime.Now;


            // ensure that the previously built entities can look up relationships
		    //selfDeferredEntitiesList.AttachApp(this);
		    // BetaDeferredEntitiesList = selfDeferredEntitiesList;
        }



        internal AppDataPackage(int appId, Log parentLog): base("App.Packge", parentLog)
	    {
	        AppId = appId;

            Relationships = new AppRelationshipManager();

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
	        if (List != null)
	            throw new Exception("can't set content-types if entities List already exist");

	        _appTypesFromRepository = RemoveAliasesForGlobalTypes(contentTypes);
	        // build types by name
	        BuildCacheForTypesByName(_appTypesFromRepository);
	    }

	    internal void Set3Entities(IEnumerable<IEntity> entList)
	    {
	        if (_appTypesFromRepository == null)
	            throw new Exception("can't set entities if content-types not set yet");

	        List = entList;
	    }


    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.App
{
	/// <inheritdoc />
	/// <summary>
	/// Cache Object for a specific App
	/// </summary>
	public partial class AppDataPackage: HasLog
	{
		#region private entities lists
		private IDictionary<int, IEntity> _publishedEntities;
		private IDictionary<int, IEntity> _draftEntities;
		#endregion

		#region public properties like AppId, Entities, List, Publisheentities, DraftEntities, 
        /// <summary>
        /// App ID
        /// </summary>
        public int AppId { get; }

		/// <summary>
		/// Gets all Entities in this App
		/// </summary>
		public IDictionary<int, IEntity> Entities { get; }

        /// <summary>
        /// The simple list of entities, used in many pipeline parts
        /// </summary>
        public IEnumerable<IEntity> List { get; } 

		/// <summary>
		/// Get all Published Entities in this App (excluding Drafts)
		/// </summary>
		public IDictionary<int, IEntity> PublishedEntities => _publishedEntities ?? (_publishedEntities = Entities.Where(e => e.Value.IsPublished).ToDictionary(k => k.Key, v => v.Value));

	    /// <summary>
		/// Get all Entities not having a Draft (Entities that are Published (not having a draft) or draft itself)
		/// </summary>
		public IDictionary<int, IEntity> DraftEntities => _draftEntities ?? (_draftEntities = Entities.Where(e => e.Value.GetDraft() == null).ToDictionary(k => k.Value.EntityId, v => v.Value));


        /// <summary>
        /// Get all Relationships between Entities
        /// </summary>
        public IEnumerable<EntityRelationshipItem> Relationships { get; }

		/// <summary>
		/// Gets the DateTime when this CacheItem was populated
		/// </summary>
		public DateTime LastRefresh { get; }
		#endregion

		/// <summary>
		/// Construct a new CacheItem with all required Items
		/// </summary>
		public AppDataPackage(
            int appId,
            IDictionary<int, IEntity> entities, 
            IEnumerable<IEntity> entList,
            IList<IContentType> contentTypes,
			IDictionary<int, Dictionary<Guid, IEnumerable<IEntity>>> metadataForGuid, 
            IDictionary<int, Dictionary<int, IEnumerable<IEntity>>> metadataForNumber,
			IDictionary<int, Dictionary<string, IEnumerable<IEntity>>> metadataForString, 
            IEnumerable<EntityRelationshipItem> relationships,
            AppDataPackageDeferredList selfDeferredEntitiesList, Log parentLog): base("App.Packge", parentLog)
		{
		    AppId = appId;
		    List = entList;
		    Entities = entities;

		    _appTypesFromRepository = RemoveAliasesForGlobalTypes(contentTypes);

            MetadataForGuid = metadataForGuid;
			MetadataForNumber = metadataForNumber;
			MetadataForString = metadataForString;

			Relationships = relationships;

			LastRefresh = DateTime.Now;

            // build types by name
            BuildCacheForTypesByName(_appTypesFromRepository);

            // ensure that the previously built entities can look up relationships
		    selfDeferredEntitiesList.AttachApp(this);
		    BetaDeferredEntitiesList = selfDeferredEntitiesList;
        }

    }
}
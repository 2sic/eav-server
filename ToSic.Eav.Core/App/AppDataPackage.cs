using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
	/// <inheritdoc />
	/// <summary>
	/// Cache Object for a specific App
	/// </summary>
	public partial class AppDataPackage: IMetadataProvider
	{
		#region Private Fields
		private IDictionary<int, IEntity> _publishedEntities;
		private IDictionary<int, IEntity> _draftEntities;
		#endregion

		#region Properties
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


        #region Metadata
        /// <summary>
        /// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyGuid
        /// </summary>
        private IDictionary<int, Dictionary<Guid, IEnumerable<IEntity>>> MetadataForGuid { get; }

		/// <summary>
		/// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyNumber
		/// </summary>
		private IDictionary<int, Dictionary<int, IEnumerable<IEntity>>> MetadataForNumber { get; }

		/// <summary>
		/// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyString
		/// </summary>
		private IDictionary<int, Dictionary<string, IEnumerable<IEntity>>> MetadataForString { get; }

        private ImmutableDictionary<int, string> MetadataTypes { get;  }

	    /// <summary>
	    /// Get AssignmentObjectTypeId by Name
	    /// </summary>
	    public int GetMetadataType(string typeName) => MetadataTypes.First(mt => mt.Value == typeName).Key;

	    public string GetMetadataType(int typeId) => MetadataTypes[typeId];


        public IEnumerable<IEntity> GetMetadata(int targetType, int key, string contentTypeName = null) => Lookup(MetadataForNumber, targetType, Convert.ToInt32(key), contentTypeName);

	    public IEnumerable<IEntity> GetMetadata(int targetType, string key, string contentTypeName = null) => Lookup(MetadataForString, targetType, key, contentTypeName);

	    public IEnumerable<IEntity> GetMetadata(int targetType, Guid key, string contentTypeName = null) => Lookup(MetadataForGuid, targetType, key, contentTypeName);

	    private IEnumerable<IEntity> Lookup<T>(IDictionary<int, Dictionary<T, IEnumerable<IEntity>>> list, int targetType, T key, string contentTypeName = null)
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            if (list.TryGetValue(targetType, out Dictionary<T, IEnumerable<IEntity>> keyGuidDictionary))
                if (keyGuidDictionary.TryGetValue(key, out IEnumerable<IEntity> entities))
                    return entities.Where(e => contentTypeName == null || e.Type.StaticName == contentTypeName);
            return new List<IEntity>();
        }
        #endregion

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
            IDictionary<int, IContentType> contentTypes,
			IDictionary<int, Dictionary<Guid, IEnumerable<IEntity>>> metadataForGuid, 
            IDictionary<int, Dictionary<int, IEnumerable<IEntity>>> metadataForNumber,
			IDictionary<int, Dictionary<string, IEnumerable<IEntity>>> metadataForString, 
            ImmutableDictionary<int, string> metadataTypes,
            IEnumerable<EntityRelationshipItem> relationships)
		{
		    AppId = appId;
		    List = entList;
		    Entities = entities;
			//ContentTypes = contentTypes;
		    _appTypesFromRepository = contentTypes;
            MetadataForGuid = metadataForGuid;
			MetadataForNumber = metadataForNumber;
			MetadataForString = metadataForString;
		    MetadataTypes = metadataTypes;
			Relationships = relationships;

			LastRefresh = DateTime.Now;

            // build types by name
            BuildTypesByNameCache(contentTypes);
		}

	}
}
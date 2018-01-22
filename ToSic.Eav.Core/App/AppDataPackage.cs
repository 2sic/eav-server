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

            LastRefresh = DateTime.Now;
	    }

	    internal void Set1MetadataManager(ImmutableDictionary<int, string> metadataTypes)
	        => Metadata = _appTypesFromRepository == null
	            ? new AppMetadataManager(metadataTypes)
	            : throw new Exception("can't set metadata if content-types are already set");


	    internal void Set2ContentTypes(IList<IContentType> contentTypes)
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

            if (Index.ContainsKey(newEntity.RepositoryId))
                throw new Exception("updating not supported yet");

            Metadata.Add((Entity)newEntity);

            Index.Add(newEntity.RepositoryId, newEntity);
	        //((List<IEntity>) List).Add(newEntity);
	    }

        /// <summary>
        /// Reset all item storages and indexes
        /// </summary>
	    internal void Reset()
	    {
	        Index.Clear();
            Relationships.Clear();
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

	    public void RebuildRelationshipIndex()
	    {
	        Relationships.Clear();
	        foreach (var entity in List)
	        foreach (var attrib in entity.Attributes.Select(a => a.Value)
                .Where(a => a is IAttribute<EntityRelationship>)
                .Cast<IAttribute<EntityRelationship>>())
	        foreach (var val in attrib.Typed[0].TypedContents.EntityIds.Where(e => e != null))
	            Relationships.Add(entity.EntityId, val);
	    }
    }
}
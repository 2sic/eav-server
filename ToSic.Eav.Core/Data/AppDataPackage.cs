using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Cache Object for a specific App
	/// </summary>
	public class AppDataPackage
	{
		#region Private Fields
		private IDictionary<int, IEntity> _publishedEntities;
		private IDictionary<int, IEntity> _draftEntities;
		#endregion

		#region Properties
		/// <summary>
		/// Gets all Entities in this App
		/// </summary>
		public IDictionary<int, IEntity> Entities { get; private set; }

        /// <summary>
        /// The simple list of entities, used in many pipeline parts
        /// </summary>
        public IEnumerable<IEntity> List { get; private set; } 

		/// <summary>
		/// Get all Published Entities in this App (excluding Drafts)
		/// </summary>
		public IDictionary<int, IEntity> PublishedEntities
		{
			get { return _publishedEntities ?? (_publishedEntities = Entities.Where(e => e.Value.IsPublished).ToDictionary(k => k.Key, v => v.Value)); }
		}
		/// <summary>
		/// Get all Entities not having a Draft (Entities that are Published (not having a draft) or draft itself)
		/// </summary>
		public IDictionary<int, IEntity> DraftEntities
		{
			get { return _draftEntities ?? (_draftEntities = Entities.Where(e => e.Value.GetDraft() == null).ToDictionary(k => k.Value.EntityId, v => v.Value)); }
		}
		/// <summary>
		/// Gets all ContentTypes in this App
		/// </summary>
		public IDictionary<int, IContentType> ContentTypes { get; private set; }
		/// <summary>
		/// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyGuid
		/// </summary>
		public IDictionary<int, Dictionary<Guid, IEnumerable<IEntity>>> AssignmentObjectTypesGuid { get; private set; }
		/// <summary>
		/// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyNumber
		/// </summary>
		public IDictionary<int, Dictionary<int, IEnumerable<IEntity>>> AssignmentObjectTypesNumber { get; private set; }
		/// <summary>
		/// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyString
		/// </summary>
		public IDictionary<int, Dictionary<string, IEnumerable<IEntity>>> AssignmentObjectTypesString { get; private set; }
		/// <summary>
		/// Get all Relationships between Entities
		/// </summary>
		public IEnumerable<EntityRelationshipItem> Relationships { get; private set; }
		/// <summary>
		/// Gets the DateTime when this CacheItem was populated
		/// </summary>
		public DateTime LastRefresh { get; private set; }
		#endregion

		/// <summary>
		/// Construct a new CacheItem with all required Items
		/// </summary>
		public AppDataPackage(
            IDictionary<int, IEntity> entities, 
            IEnumerable<IEntity> entList,
            IDictionary<int, IContentType> contentTypes,
			IDictionary<int, Dictionary<Guid, IEnumerable<IEntity>>> assignmentObjectTypesGuid, 
            IDictionary<int, Dictionary<int, IEnumerable<IEntity>>> assignmentObjectTypesNumber,
			IDictionary<int, Dictionary<string, IEnumerable<IEntity>>> assignmentObjectTypesString, 
            IEnumerable<EntityRelationshipItem> relationships)
		{
		    List = entList;
		    Entities = entities;// entList.ToDictionary(e => e.EntityId, e => e); // can't use the pure to-dictionary, because I seem to have duplicate entries - probably because of draft-items?
			ContentTypes = contentTypes;
			AssignmentObjectTypesGuid = assignmentObjectTypesGuid;
			AssignmentObjectTypesNumber = assignmentObjectTypesNumber;
			AssignmentObjectTypesString = assignmentObjectTypesString;
			Relationships = relationships;

			LastRefresh = DateTime.Now;
		}
	}
}
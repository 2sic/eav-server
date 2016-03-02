using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using ToSic.Eav.Implementations.ValueConverter;
using Microsoft.Practices.Unity;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Represents an Entity
	/// </summary>
	public class Entity : IEntity
    {
        #region Basic properties like EntityId, Guid, IsPublished etc.
        /// <summary>
        /// Id as an int
        /// </summary>
		public int EntityId { get; set; } // todo: set shouldn't be pbulic
        /// <summary>
        /// Id of this item inside the repository. Can be different than the real Id, because it may be a temporary version of this content-item
        /// </summary>
		public int RepositoryId { get; internal set; }
        /// <summary>
        /// Id as GUID
        /// </summary>
		public Guid EntityGuid { get; internal set; }
        /// <summary>
        /// Offical title of this content-item
        /// </summary>
		public IAttribute Title { get;  set; } // todo: title shouldn' have a public set, had to open this while refactoring
        /// <summary>
        /// List of all attributes
        /// </summary>
		public Dictionary<string, IAttribute> Attributes { get; internal set; }
        /// <summary>
        /// Type-definition of this content-item
        /// </summary>
		public IContentType Type { get; internal set; }
        /// <summary>
        /// Modified date/time
        /// </summary>
		public DateTime Modified { get; internal set; }
        /// <summary>
        /// Relationship-helper object, important to navigate to children and parents
        /// </summary>
		[ScriptIgnore]
		public IRelationshipManager Relationships { get; internal set; }
        /// <summary>
        /// Published/Draft status. If not published, it may be invisble, but there may also be another item visible ATM
        /// </summary>
		public bool IsPublished { get; internal set; }

        /// <summary>
        /// Internal value - ignore for now
        /// </summary>
        [Obsolete("You should use Metadata.TargetType instead")]
		public int AssignmentObjectTypeId { get; internal set; }

        public IMetadata Metadata { get; set; }
        /// <summary>
        /// If this entity is published and there is a draft of it, then it can be navigated through DraftEntity
        /// </summary>
		public IEntity DraftEntity { get; set; }
        /// <summary>
        /// If this entity is draft and there is a published edition, then it can be navigated through PublishedEntity
        /// </summary>
        public IEntity PublishedEntity { get; set; }

        /// <summary>
        /// Shorhand accessor to retrieve an attribute
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
		public IAttribute this[string attributeName]
		{
			get { return (Attributes.ContainsKey(attributeName)) ? Attributes[attributeName] : null; }
		}
        #endregion

        /// <summary>
		/// Create a new Entity. Used to create InMemory Entities that are not persisted to the EAV SqlStore.
		/// </summary>
		public Entity(int entityId, string contentTypeName, IDictionary<string, object> values, string titleAttribute)
		{
			EntityId = entityId;
			Type = new ContentType(contentTypeName);
			Attributes = AttributeHelperTools.GetTypedDictionaryForSingleLanguage(values, titleAttribute);
			try
			{
				Title = Attributes[titleAttribute];
			}
			catch (KeyNotFoundException)
			{
				throw new KeyNotFoundException(string.Format("The Title Attribute with Name \"{0}\" doesn't exist in the Entity-Attributes.", titleAttribute));
			}
			AssignmentObjectTypeId = Constants.DefaultAssignmentObjectTypeId;
			IsPublished = true;
			Relationships = new RelationshipManager(this, new EntityRelationshipItem[0]);
		}

		/// <summary>
		/// Create a new Entity
		/// </summary>
		public Entity(Guid entityGuid, int entityId, int repositoryId, IMetadata metadata /* int assignmentObjectTypeId */, IContentType type, bool isPublished, IEnumerable<EntityRelationshipItem> allRelationships, DateTime modified, string owner)
		{
			EntityId = entityId;
			EntityGuid = entityGuid;
		    Metadata = metadata;
		    AssignmentObjectTypeId = Metadata.TargetType;// assignmentObjectTypeId;
			Attributes = new Dictionary<string, IAttribute>(StringComparer.OrdinalIgnoreCase); // 2015-04-24 added, maybe a risk but should help with tokens
			Type = type;
			IsPublished = isPublished;
			RepositoryId = repositoryId;
			Modified = modified;

			if (allRelationships == null)
				allRelationships = new List<EntityRelationshipItem>();
			Relationships = new RelationshipManager(this, allRelationships);

		    Owner = owner;
		}

		/// <summary>
		/// Create a new Entity based on an Entity and Attributes
		/// </summary>
		public Entity(IEntity entity, Dictionary<string, IAttribute> attributes, IEnumerable<EntityRelationshipItem> allRelationships, string owner)
		{
			EntityId = entity.EntityId;
			EntityGuid = entity.EntityGuid;
			AssignmentObjectTypeId = entity.AssignmentObjectTypeId;
			Type = entity.Type;
			Title = entity.Title;
			IsPublished = entity.IsPublished;
			Attributes = attributes;
			RepositoryId = entity.RepositoryId;
			Relationships = new RelationshipManager(this, allRelationships);

            Owner = owner;
        }

        /// <summary>
        /// The draft entity fitting this published entity
        /// </summary>
        /// <returns></returns>
        public IEntity GetDraft()
		{
			return DraftEntity;
		}

        /// <summary>
        /// The published entity of this draft entity
        /// </summary>
        /// <returns></returns>
        public IEntity GetPublished()
		{
			return PublishedEntity;
		}

		/// <summary>
		/// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
		/// Assumes default preferred language
		/// </summary>
		/// <param name="attributeName">Name of the attribute or virtual attribute</param>
		/// <param name="resolveHyperlinks"></param>
		/// <returns></returns>
		public object GetBestValue(string attributeName, bool resolveHyperlinks = false)
	    {
	        return GetBestValue(attributeName, new string[0], resolveHyperlinks);
	    }

		/// <summary>
		/// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
		/// Automatically resolves the language-variations as well based on the list of preferred languages
		/// </summary>
		/// <param name="attributeName">Name of the attribute or virtual attribute</param>
		/// <param name="dimensions">List of languages</param>
		/// <param name="resolveHyperlinks"></param>
		/// <returns>An object OR a null - for example when retrieving the title and no title exists</returns>
		public object GetBestValue(string attributeName, string[] dimensions, bool resolveHyperlinks = false) 
        {
            object result = null;
			IAttribute attribute = null;

            if (Attributes.ContainsKey(attributeName))
            {
                attribute = Attributes[attributeName];
                result = attribute[dimensions];
            }
            else
            {
                switch (attributeName)
                {
                    case "EntityTitle":
                        result = Title?[dimensions];
		                attribute = Title;
                        break;
                    case "EntityId":
                        result = EntityId;
                        break;
                    case "EntityGuid":
                        result = EntityGuid;
                        break;
                    case "EntityType":
                        result = Type.Name;
                        break;
                    case "IsPublished":
                        result = IsPublished;
                        break;
                    case "Modified":
                        result = Modified;
                        break;
                    default:
                        result = null;
                        break;
                }
            }

			if (resolveHyperlinks && attribute != null && result is string && attribute.Type == "Hyperlink")
			{
				var vc = Factory.Container.Resolve<IEavValueConverter>();
				result = vc.Convert(ConversionScenario.GetFriendlyValue, "Hyperlink", (string)result);
			}

            return result;
        }

	    public string Owner { get; internal set; }
    }
}

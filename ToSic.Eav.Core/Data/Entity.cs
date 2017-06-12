using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.Interfaces;
#pragma warning disable 618

// ReSharper disable once CheckNamespace
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
		public int EntityId { get; internal set; } 
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
		public IAttribute Title { get;  internal set; }
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
        /// Internal value - ignore for now
        /// </summary>
        [Obsolete("You should use Metadata.TargetType instead")]
        public int AssignmentObjectTypeId => Metadata.TargetType; //{ get; internal set; }

        public IMetadata Metadata { get; set; }

        /// <summary>
        /// Owner of this entity
        /// </summary>
        public string Owner { get; internal set; }

        #region IsPublished, DratEntity, PublishedEntity
        /// <summary>
        /// Published/Draft status. If not published, it may be invisble, but there may also be another item visible ATM
        /// </summary>
        public bool IsPublished { get; internal set; }
        /// <summary>
        /// If this entity is published and there is a draft of it, then it can be navigated through DraftEntity
        /// </summary>
		public IEntity DraftEntity { get; set; }
        /// <summary>
        /// If this entity is draft and there is a published edition, then it can be navigated through PublishedEntity
        /// </summary>
        public IEntity PublishedEntity { get; set; }
        #endregion

        /// <summary>
        /// Shorhand accessor to retrieve an attribute
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public IAttribute this[string attributeName] => Attributes.ContainsKey(attributeName) ? Attributes[attributeName] : null;

        #endregion

        #region various constructors to create entities

        /// <summary>
        /// Create a new Entity. Used to create InMemory Entities that are not persisted to the EAV SqlStore.
        /// </summary>
        public Entity(int entityId, string contentTypeName, IDictionary<string, object> values, string titleAttribute, DateTime? modified = null)
		{
			EntityId = entityId;
			Type = new ContentType(contentTypeName);
			Attributes = GetTypedAttributesSimple(values, titleAttribute);
			try
			{
				Title = Attributes[titleAttribute];
			}
			catch (KeyNotFoundException)
			{
				throw new KeyNotFoundException($"The Title Attribute with Name \"{titleAttribute}\" doesn't exist in the Entity-Attributes.");
			}
            Metadata = new Metadata();
		    IsPublished = true;
            if (modified.HasValue)
                Modified = modified.Value;
			Relationships = new RelationshipManager(this, new EntityRelationshipItem[0]);
		}

		/// <summary>
		/// Create a new Entity from a data store (usually SQL backend)
		/// </summary>
		public Entity(Guid entityGuid, int entityId, int repositoryId, IMetadata isMetadata, IContentType type, bool isPublished, IEnumerable<EntityRelationshipItem> allRelationships, DateTime modified, string owner)
		{
			EntityId = entityId;
			EntityGuid = entityGuid;
		    Metadata = isMetadata;
		    Attributes = new Dictionary<string, IAttribute>(StringComparer.OrdinalIgnoreCase);
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
		    Metadata = ((Metadata)entity.Metadata).CloneIsMetadata();
			//AssignmentObjectTypeId = entity.AssignmentObjectTypeId;
			Type = entity.Type;
			Title = entity.Title;
			IsPublished = entity.IsPublished;
			Attributes = attributes;
			RepositoryId = entity.RepositoryId;
			Relationships = new RelationshipManager(this, allRelationships);

            Owner = owner;
        }

        #region Helper to assemble an entity from a dictionary of properties
        /// <summary>
        /// Convert a NameValueCollection-Like List to a Dictionary of IAttributes
        /// </summary>
        private static Dictionary<string, IAttribute> GetTypedAttributesSimple(IDictionary<string, object> attributes, string titleAttributeName)
        {
            var result = new Dictionary<string, IAttribute>(StringComparer.OrdinalIgnoreCase);

            foreach (var attribute in attributes)
            {
                var attributeType = GetAttributeTypeName(attribute.Value);
                var baseModel = new AttributeDefinition(attribute.Key, attributeType, attribute.Key == titleAttributeName, 0, 0);
                var attributeModel = baseModel.CreateAttribute();// CreateTypedAttribute(baseModel);
                var valuesModelList = new List<IValue>();
                if (attribute.Value != null)
                {
                    var valueModel = Value.Build(baseModel.Type, attribute.Value.ToString(), null);
                    valuesModelList.Add(valueModel);
                }

                attributeModel.Values = valuesModelList;

                result[attribute.Key] = attributeModel;
            }

            return result;

            // helper to get text-name of the type
            string GetAttributeTypeName(object value)
            {
                if (value is DateTime)
                    return "DateTime";
                if (value is decimal || value is int || value is double)
                    return "Number";
                if (value is bool)
                    return "Boolean";
                return "String";
            }
        }

        #endregion

        #endregion

        #region GetDraft and GetPublished
        /// <summary>
        /// The draft entity fitting this published entity
        /// </summary>
        /// <returns></returns>
        public IEntity GetDraft() => DraftEntity;

	    /// <summary>
	    /// The published entity of this draft entity
	    /// </summary>
	    /// <returns></returns>
	    public IEntity GetPublished() => PublishedEntity;

        #endregion

        #region GetBestValue and GetTitle

        /// <summary>
        /// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
        /// Assumes default preferred language
        /// </summary>
        /// <param name="attributeName">Name of the attribute or virtual attribute</param>
        /// <param name="resolveHyperlinks"></param>
        /// <returns></returns>
        public object GetBestValue(string attributeName, bool resolveHyperlinks = false)
	        => GetBestValue(attributeName, new string[0], resolveHyperlinks);
	    



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
            object result;
			IAttribute attribute = null;

            if (Attributes.ContainsKey(attributeName))
            {
                attribute = Attributes[attributeName];
                result = attribute[dimensions];
            }
            else
            {
                switch (attributeName.ToLower())
                {
                    case Constants.EntityFieldTitle:
                        result = Title?[dimensions];
		                attribute = Title;
                        break;
                    case Constants.EntityFieldId:
                        result = EntityId;
                        break;
                    case Constants.EntityFieldGuid:
                        result = EntityGuid;
                        break;
                    case Constants.EntityFieldType:
                        result = Type.Name;
                        break;
                    case Constants.EntityFieldIsPublished:
                        result = IsPublished;
                        break;
                    case Constants.EntityFieldModified:
                        result = Modified;
                        break;
                    default:
                        result = null;
                        break;
                }
            }

			if (resolveHyperlinks && attribute != null && result is string && attribute.Type == Constants.Hyperlink)
			{
				var vc = Factory.Resolve<IEavValueConverter>();
				result = vc.Convert(ConversionScenario.GetFriendlyValue, Constants.Hyperlink, (string)result);
			}

            return result;
        }


        /// <summary>
        /// Best way to get the current entities title
        /// </summary>
        /// <returns>The entity title as a string</returns>
	    public string GetBestTitle() => GetBestTitle(null, 0);

        public string GetBestTitle(string[] dimensions) => GetBestTitle(dimensions, 0);

        /// <summary>
        /// Try to look up the title while also checking titles built with entities,
        /// but make sure we don't recurse forever
        /// </summary>
        /// <param name="dimensions"></param>
        /// <param name="recursionCount"></param>
        /// <returns></returns>
        internal string GetBestTitle(string[] dimensions, int recursionCount)
        {
            var bestTitle = GetBestValue(Constants.EntityFieldTitle, dimensions);

            // in case the title is an entity-picker and has items, try to ask it for the title
            // note that we're counting recursions, just to be sure it won't loop forever
            var maybeRelationship = bestTitle as EntityRelationship;
            if (recursionCount < 3 && (maybeRelationship?.Any() ?? false))
                bestTitle = (maybeRelationship.FirstOrDefault() as Entity)?
                    .GetBestTitle(dimensions, recursionCount + 1) 
                    ?? bestTitle;

            return bestTitle?.ToString();
        }

        #endregion
    }
}

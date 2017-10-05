using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    public class Entity: EntityLight, IEntity
    {

        #region Basic properties like EntityId, Guid, IsPublished etc.
        /// <summary>
        /// Offical title of this content-item
        /// </summary>
        public new IAttribute Title {
            get
            {
                if (TitleFieldName == null) return null;
                return Attributes?.ContainsKey(TitleFieldName) ?? false ? Attributes[TitleFieldName] : null;
            }
        }


        /// <summary>
        /// List of all attributes
        /// </summary>
        public Dictionary<string, IAttribute> Attributes {
            get { return _attributes ?? (_attributes = LightAttributesForInternalUseOnlyForNow.ConvertToAttributes()); }
            set { _attributes = value; } }

        private Dictionary<string, IAttribute> _attributes;

        #region IsPublished, DratEntity, PublishedEntity
        /// <summary>
        /// Id of this item inside the repository. Can be different than the real Id, because it may be a temporary version of this content-item
        /// </summary>
        public int RepositoryId { get; internal set; }

        /// <summary>
        /// Published/Draft status. If not published, it may be invisble, but there may also be another item visible ATM
        /// </summary>
        public bool IsPublished { get; set; } = true;

        /// <summary>
        /// If this entity is published and there is a draft of it, then it can be navigated through DraftEntity
        /// </summary>
		public IEntity DraftEntity { get; set; }

        /// <summary>
        /// If this entity is draft and there is a published edition, then it can be navigated through PublishedEntity
        /// </summary>
        public IEntity PublishedEntity { get; internal set; }

        internal int? PublishedEntityId { get; set; } = null;
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

        #endregion

        #region internal properties to use object-access instead of IAttribute access

        /// <summary>
        /// This determines if the access to the properties will use light-objects, or IAttributes containing multi-language objects
        /// </summary>
        private readonly bool _useLightModel;
        #endregion

        #region simple direct accessors

        /// <inheritdoc />
        /// <summary>
        /// Shorhand accessor to retrieve an attribute
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        IAttribute IEntity.this[string attributeName] => Attributes.ContainsKey(attributeName) ? Attributes[attributeName] : null;

        #endregion

        public Entity(int appId, int entityId, string contentTypeName, Dictionary<string, object> values, string titleAttribute = null, DateTime? modified = null) : base(appId, entityId, contentTypeName, values, titleAttribute, modified)
        {
            if (values.All(x => x.Value is IAttribute))
                Attributes = values.ToDictionary(x => x.Key, x => x.Value as IAttribute);
            else
                _useLightModel = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a brand new Entity. 
        /// Mainly used for entities which are created for later saving
        /// </summary>
        public Entity(int appId, Guid entityGuid, string contentTypeName, Dictionary<string, object> values) : base(appId, entityGuid, contentTypeName, values)
        {
            if (values.All(x => x.Value is IAttribute))
                Attributes = values.ToDictionary(x => x.Key, x => x.Value as IAttribute);
            else
                _useLightModel = true;
        }

        /// <summary>
        /// Create a new Entity from a data store (usually SQL backend)
        /// </summary>
        public Entity(int appId, Guid entityGuid, int entityId, int repositoryId, IMetadata isMetadata, IContentType type, bool isPublished, IEnumerable<EntityRelationshipItem> allRelationships, DateTime modified, string owner, int version)
        {
            AppId = appId;
            EntityId = entityId;
            Version = version;
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
        /// Used in the Attribute-Filter, which generates a new entity with less properties
        /// </summary>
        public Entity(IEntity entity, Dictionary<string, IAttribute> attributes, IEnumerable<EntityRelationshipItem> allRelationships)
        {
            AppId = entity.AppId;
            EntityId = entity.EntityId;
            EntityGuid = entity.EntityGuid;
            Metadata = ((Metadata)entity.Metadata).CloneIsMetadata();
            Type = entity.Type;
            TitleFieldName = entity.Title?.Name;
            IsPublished = entity.IsPublished;
            Attributes = attributes;
            RepositoryId = entity.RepositoryId;
            Relationships = new RelationshipManager(this, allRelationships);
            Owner = entity.Owner;
        }

        /// <summary>
        /// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
        /// Assumes default preferred language
        /// </summary>
        /// <param name="attributeName">Name of the attribute or virtual attribute</param>
        /// <param name="resolveHyperlinks"></param>
        /// <returns></returns>
        public new object GetBestValue(string attributeName, bool resolveHyperlinks = false)
            => GetBestValue(attributeName, new string[0], resolveHyperlinks);



        /// <summary>
        /// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
        /// Automatically resolves the language-variations as well based on the list of preferred languages
        /// </summary>
        /// <param name="attributeName">Name of the attribute or virtual attribute</param>
        /// <param name="languages">List of languages</param>
        /// <param name="resolveHyperlinks"></param>
        /// <returns>An object OR a null - for example when retrieving the title and no title exists</returns>
        public object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks = false)
        {
            if (_useLightModel)
                return base.GetBestValue(attributeName, resolveHyperlinks);

            object result;
            IAttribute attribute;

            if (Attributes.ContainsKey(attributeName))
            {
                attribute = Attributes[attributeName];
                result = attribute[languages];
            }
            else switch (attributeName.ToLower())
            {
                case Constants.EntityFieldTitle:
                    result = Title?[languages];
                    attribute = Title;
                    break;
                case Constants.EntityFieldIsPublished:
                    // directly return internal properties, don't allow further Link resolution
                    return IsPublished;
                default:
                    // directly return internal properties, don't allow further Link resolution
                    return GetInternalPropertyByName(attributeName);
            }

            if (resolveHyperlinks && attribute?.Type == Constants.Hyperlink)
                result = TryToResolveLink(result);

            return result;
        }

        /// <summary>
        /// Best way to get the current entities title
        /// </summary>
        /// <returns>The entity title as a string</returns>
        public new string GetBestTitle() => GetBestTitle(null, 0);

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

        public int Version { get; }
    }
}

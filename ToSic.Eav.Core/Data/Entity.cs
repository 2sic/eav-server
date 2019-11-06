using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data
{
    public class Entity: EntityLight, IEntity
    {

        #region Basic properties like EntityId, Guid, IsPublished etc.
        /// <summary>
        /// Official title of this content-item
        /// </summary>
        public new IAttribute Title => TitleFieldName == null
            ? null
            : (Attributes?.ContainsKey(TitleFieldName) ?? false ? Attributes[TitleFieldName] : null);


        /// <summary>
        /// List of all attributes
        /// </summary>
        public Dictionary<string, IAttribute> Attributes {
            get => _attributes ?? (_attributes = LightAttributesForInternalUseOnlyForNow.ConvertToAttributes());
            set => _attributes = value;
        }

        private Dictionary<string, IAttribute> _attributes;

        #region IsPublished, DratEntity, PublishedEntity
        /// <inheritdoc />
        public int RepositoryId { get; internal set; }

        /// <inheritdoc />
        /// <summary>
        /// Published/Draft status. If not published, it may be invisible, but there may also be another item visible ATM
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
        /// <inheritdoc />
        public IEntity GetDraft() => DraftEntity;

        /// <inheritdoc />
        public IEntity GetPublished() => PublishedEntity;

        #endregion

        #endregion

        #region internal properties to use object-access instead of IAttribute access

        /// <summary>
        /// This determines if the access to the properties will use light-objects, or IAttributes containing multi-language objects
        /// </summary>
        private bool _useLightModel;
        #endregion

        #region simple direct accessors

        /// <inheritdoc />
        /// <summary>
        /// Shorthand accessor to retrieve an attribute
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        IAttribute IEntity.this[string attributeName] => Attributes.ContainsKey(attributeName) ? Attributes[attributeName] : null;

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// special blank constructor for entity-builders
        /// </summary>
        internal Entity() { }

        public Entity(int appId, int entityId, Guid entityGuid, string contentType, Dictionary<string, object> values, string titleAttribute = null, DateTime? modified = null)
            : base(appId, entityId, entityGuid, new ContentType(appId, contentType), values, titleAttribute, modified)
        {
            MapAttributesInConstructor(values);
        }

        public Entity(int appId, int entityId, IContentType contentType, Dictionary<string, object> values, string titleAttribute = null, DateTime? modified = null, Guid? entityGuid = null) 
            : base(appId, entityId, entityGuid, contentType, values, titleAttribute, modified)
        {
            MapAttributesInConstructor(values);
        }

        private void MapAttributesInConstructor(Dictionary<string, object> values)
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
        public Entity(int appId, Guid entityGuid, IContentType contentType, Dictionary<string, object> values) 
            : this(appId, 0, contentType, values, entityGuid: entityGuid)
        {}
        

        /// <inheritdoc />
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

            if (resolveHyperlinks && attribute?.Type == Constants.DataTypeHyperlink && result is string strResult)
                result = TryToResolveLink(EntityGuid, strResult);

            return result;
        }

        public new TVal GetBestValue<TVal>(string name, bool resolveHyperlinks = false)
            => ChangeTypeOrDefault<TVal>(GetBestValue(name, resolveHyperlinks));

        public TVal GetBestValue<TVal>(string name, string[] languages, bool resolveHyperlinks = false)
            => ChangeTypeOrDefault<TVal>(GetBestValue(name, resolveHyperlinks));

        public object PrimaryValue(string attributeName, bool resolveHyperlinks = false)
            => GetBestValue(attributeName, new string[0], resolveHyperlinks);

        public TVal PrimaryValue<TVal>(string attributeName, bool resolveHyperlinks = false)
            => GetBestValue<TVal>(attributeName, new string[0], resolveHyperlinks);

        /// <inheritdoc />
        public new string GetBestTitle() => GetBestTitle(null, 0);

        /// <inheritdoc />
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

        /// <inheritdoc />
        public int Version { get; internal set; } = 1;




        #region Metadata & Permissions

        public IMetadataOfItem Metadata => _metadata ?? (_metadata =
                                               new MetadataOf<Guid>(Constants.MetadataForEntity, EntityGuid, DeferredLookupData));
        private IMetadataOfItem _metadata;
        internal IDeferredEntitiesList DeferredLookupData = null;

        public IEnumerable<IEntity> Permissions => Metadata.Permissions;
        #endregion


        #region Obsolete / Special breaking changes

        //public object Value 
        //    => throw new Exception("Error: You're seeing this because of a breaking change " +
        //                           "in EAV 4.5 / 2sxc 9.8. Please read this to fix: " +
        //                           "https://2sxc.org/en/blog/post/fixing-the-breaking-change-in-2sxc-9-8-list-instead-of-dictionary");

        #endregion

        public object Value(string field, bool resolve = true)
            => GetBestValue(field, new[] { Thread.CurrentThread.CurrentCulture.Name }, resolve);

        public T Value<T>(string field, bool resolve = true)
            => GetBestValue<T>(field, new[] { Thread.CurrentThread.CurrentCulture.Name }, resolve);


        #region IEntity Queryable / Quick
        public List<IEntity> Children(string field = null, string type = null)
        {
            var list = Relationships
                .FindChildren(field, type)
                .ToList();
            return list;
        }

        public List<IEntity> Parents(string type = null, string field = null)
        {
            var list = Relationships
                .FindParents(type, field)
                .ToList();
            return list;

        }

        #endregion
    }
}

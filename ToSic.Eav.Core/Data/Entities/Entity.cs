using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// A basic unit / item of data. Has many <see cref="IAttribute{T}"/>s which then contains <see cref="IValue{T}"/>s which are multi-language. 
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi, always use IEntity")]

    public class Entity: EntityLight, IEntity
    {

        #region Basic properties like EntityId, Guid, IsPublished etc.
        /// <inheritdoc />
        public new IAttribute Title => TitleFieldName == null
            ? null
            : (Attributes?.ContainsKey(TitleFieldName) ?? false ? Attributes[TitleFieldName] : null);



        /// <inheritdoc />
        public Dictionary<string, IAttribute> Attributes {
            get => _attributes ?? (_attributes = LightAttributesForInternalUseOnlyForNow.ConvertToAttributes());
            set => _attributes = value;
        }

        private Dictionary<string, IAttribute> _attributes;

        #region IsPublished, DratEntity, PublishedEntity
        /// <inheritdoc />
        [PrivateApi]
        public int RepositoryId { get; internal set; }

        /// <inheritdoc />
        public bool IsPublished { get; set; } = true;

        /// <summary>
        /// If this entity is published and there is a draft of it, then it can be navigated through DraftEntity
        /// </summary>
        [PrivateApi]
		internal IEntity DraftEntity { get; set; }

        /// <summary>
        /// If this entity is draft and there is a published edition, then it can be navigated through PublishedEntity
        /// </summary>
        internal IEntity PublishedEntity { get; set; }

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
        public new IAttribute this[string attributeName] => Attributes.ContainsKey(attributeName) ? Attributes[attributeName] : null;

        #endregion


        /// <inheritdoc />
        /// <summary>
        /// special blank constructor for entity-builders
        /// </summary>
        [PrivateApi]
        internal Entity() { }

        /// <summary>
        /// Special constructor for importing new entities without a known content-type
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="entityId"></param>
        /// <param name="entityGuid"></param>
        /// <param name="contentType"></param>
        /// <param name="values"></param>
        [PrivateApi]
        public Entity(int appId, int entityId, Guid entityGuid, string contentType, Dictionary<string, object> values) 
            : base(appId, entityId, entityGuid, new ContentType(appId, contentType), values, null, null)
        {
            MapAttributesInConstructor(values);
        }

        [PrivateApi]
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
        [PrivateApi]
        public Entity(int appId, Guid entityGuid, IContentType contentType, Dictionary<string, object> values) 
            : this(appId, 0, contentType, values, entityGuid: entityGuid)
        {}
        

        /// <inheritdoc />
        public new object GetBestValue(string attributeName, bool resolveHyperlinks = false)
            => GetBestValue(attributeName, new string[0], resolveHyperlinks);


        /// <inheritdoc />
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

        /// <inheritdoc />
        public new TVal GetBestValue<TVal>(string name, bool resolveHyperlinks = false)
            => ChangeTypeOrDefault<TVal>(GetBestValue(name, resolveHyperlinks));

        /// <inheritdoc />
        public TVal GetBestValue<TVal>(string name, string[] languages, bool resolveHyperlinks = false)
            => ChangeTypeOrDefault<TVal>(GetBestValue(name, resolveHyperlinks));

        /// <inheritdoc />
        [PrivateApi("not sure yet if this is final")]
        public object PrimaryValue(string attributeName, bool resolveHyperlinks = false)
            => GetBestValue(attributeName, new string[0], resolveHyperlinks);

        /// <inheritdoc />
        [PrivateApi("not sure yet if this is final")]
        public TVal PrimaryValue<TVal>(string attributeName, bool resolveHyperlinks = false)
            => GetBestValue<TVal>(attributeName, new string[0], resolveHyperlinks);

        /// <inheritdoc />
        public new string GetBestTitle() => GetBestTitle(null, 0);

        /// <inheritdoc />
        public string GetBestTitle(string[] dimensions) => GetBestTitle(dimensions, 0);


        /// <inheritdoc />
        internal string GetBestTitle(string[] dimensions, int recursionCount)
        {
            var bestTitle = GetBestValue(Constants.EntityFieldTitle, dimensions);

            // in case the title is an entity-picker and has items, try to ask it for the title
            // note that we're counting recursions, just to be sure it won't loop forever
            var maybeRelationship = bestTitle as IEnumerable<IEntity>;
            if (recursionCount < 3 && (maybeRelationship?.Any() ?? false))
                bestTitle = (maybeRelationship.FirstOrDefault() as Entity)?
                    .GetBestTitle(dimensions, recursionCount + 1)
                    ?? bestTitle;

            return bestTitle?.ToString();
        }

        /// <inheritdoc />
        public int Version { get; internal set; } = 1;




        #region Metadata & Permissions

        /// <inheritdoc />
        public IMetadataOf Metadata => _metadata ?? (_metadata =
                                               new MetadataOf<Guid>(Constants.MetadataForEntity, EntityGuid, DeferredLookupData));
        private IMetadataOf _metadata;
        internal IHasMetadataSource DeferredLookupData = null;

        /// <inheritdoc />
        public IEnumerable<Permission> Permissions => Metadata.Permissions;
        #endregion


        /// <inheritdoc />
        [PrivateApi("don't publish yet, not really final")]
        public object Value(string field, bool resolve = true)
            => GetBestValue(field, new[] { Thread.CurrentThread.CurrentCulture.Name }, resolve);

        /// <inheritdoc />
        [PrivateApi("don't publish yet, not really final")]
        public T Value<T>(string field, bool resolve = true)
            => GetBestValue<T>(field, new[] { Thread.CurrentThread.CurrentCulture.Name }, resolve);


        #region IEntity Queryable / Quick
        /// <inheritdoc />
        public List<IEntity> Children(string field = null, string type = null)
        {
            var list = Relationships
                .FindChildren(field, type)
                .ToList();
            return list;
        }

        /// <inheritdoc />
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

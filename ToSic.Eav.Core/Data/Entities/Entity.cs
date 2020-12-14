using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Documentation;
using ToSic.Eav.Generics;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// A basic unit / item of data. Has many <see cref="IAttribute{T}"/>s which then contains <see cref="IValue{T}"/>s which are multi-language. 
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi, always use IEntity")]

    public partial class Entity: EntityLight, IEntity
    {

        #region Basic properties like EntityId, Guid, IsPublished etc.
        /// <inheritdoc />
        public new IAttribute Title => TitleFieldName == null
            ? null
            : Attributes?.ContainsKey(TitleFieldName) ?? false ? Attributes[TitleFieldName] : null;



        /// <inheritdoc />
        public Dictionary<string, IAttribute> Attributes
        {
            get => _attributes ?? (_attributes = LightAttributesForInternalUseOnlyForNow.ConvertToInvariantDic());
            set => _attributes = (value?.ToInvariant() ?? new Dictionary<string, IAttribute>()).ToInvariant();
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
        /// Special constructor for importing-new/creating-external entities without a known content-type
        /// </summary>
        [PrivateApi]
        public Entity(int appId, int entityId, Guid entityGuid, string contentType, Dictionary<string, object> values, string titleAttribute = null, DateTime? modified = null) 
            : base(appId, entityId, entityGuid, new ContentType(appId, contentType), values, titleAttribute, modified)
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
                Attributes = values
                    .ToDictionary(x => x.Key, x => x.Value as IAttribute);
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

        [PrivateApi("Testing / wip #IValueConverter")]
        public new object GetBestValue(string attributeName) => GetBestValue(attributeName, new string[0]);



        /// <inheritdoc />
        public object GetBestValue(string attributeName, string[] languages) 
            => _useLightModel 
                ? base.GetBestValue(attributeName) 
                : GetBestValueAndType(attributeName, languages, out _);

        public string[] ExtendDimsWithDefault(string[] dimensions)
        {
            // empty list - add the default dimension
            if (dimensions == null || dimensions.Length == 0) return new[] { null as string };

            // list already has a default at the end, don't change

            // we have dimensions but no default, add it
            if (dimensions.Last() == default) return dimensions;
            var newDims = dimensions.ToList();
            newDims.Add(default);
            return newDims.ToArray();
        }

        private object GetBestValueAndType(string attributeName, string[] languages, out string attributeType)
        {
            languages = ExtendDimsWithDefault(languages);

            object result;

            //attributeName = attributeName.ToLowerInvariant();
            if (Attributes.ContainsKey(attributeName))
            {
                var attribute = Attributes[attributeName];
                result = attribute[languages];
                attributeType = attribute.Type;
            }
            else if (attributeName == Constants.EntityFieldTitle)
            {
                result = Title?[languages];
                var attribute = Title;
                attributeType = attribute?.Type;
            }
            else
            {
                attributeType = Constants.EntityFieldIsVirtual;
                // directly return internal properties, don't allow further Link resolution
                result = attributeName == Constants.EntityFieldIsPublished
                    ? IsPublished
                    : GetInternalPropertyByName(attributeName);
            }

            return result;
        }


        // 2020-10-30 trying to drop uses with ResolveHyperlinks
        ///// <inheritdoc />
        //public new TVal GetBestValue<TVal>(string name, bool resolveHyperlinks/* = false*/)
        //    => ChangeTypeOrDefault<TVal>(GetBestValue(name, resolveHyperlinks));

        [PrivateApi("Testing / wip #IValueConverter")]
        public new TVal GetBestValue<TVal>(string name) => ChangeTypeOrDefault<TVal>(GetBestValue(name));

        /// <inheritdoc />
        public TVal GetBestValue<TVal>(string name, string[] languages) => ChangeTypeOrDefault<TVal>(GetBestValue(name, languages));



        /// <inheritdoc />
        [PrivateApi("not sure yet if this is final - NEW")]
        public object PrimaryValue(string attributeName)
            => GetBestValue(attributeName, new string[0]);

        /// <inheritdoc />
        [PrivateApi("not sure yet if this is final - NEW")]
        public TVal PrimaryValue<TVal>(string attributeName)
            => GetBestValue<TVal>(attributeName, new string[0]);

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
        public object Value(string field)
            => GetBestValue(field, new[] { IZoneCultureResolverExtensions.ThreadCultureNameNotGood() });

        /// <inheritdoc />
        [PrivateApi("don't publish yet, not really final")]
        public T Value<T>(string field)
            => ChangeTypeOrDefault<T>(GetBestValue(field, new[] { IZoneCultureResolverExtensions.ThreadCultureNameNotGood() }));


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

using System.Collections.Generic;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.Data
{
    /// <inheritdoc cref="AttributeBase" />
    /// <summary>
    /// Defines an attribute with name and the type this attribute has. Part of of a <see cref="IContentType"/> definition.
    /// </summary>
    [PublicApi]
    public class ContentTypeAttribute : AttributeBase, IContentTypeAttribute
    {
        /// <inheritdoc />
        public int AppId { get; }

        /// <inheritdoc />
        public int AttributeId { get; set; }

        /// <inheritdoc />
        public int SortOrder { get; internal set; }

        /// <inheritdoc />
        public bool IsTitle { get; set; }

        private readonly IHasMetadataSource _metaOfThisApp;

        /// <inheritdoc />
        /// <summary>
        /// Extended constructor when also storing the persistence Id
        /// </summary>
        public ContentTypeAttribute(int appId, string name, string type, bool isTitle, int attributeId, int sortOrder, IHasMetadataSource metaProvider = null, int parentApp = 0) : base(name, type)
        {
            AppId = appId;
            IsTitle = isTitle;
            AttributeId = attributeId;
            SortOrder = sortOrder;
            _metaOfThisApp = metaProvider;
            _isShared = parentApp != 0;
            _parentAppId = parentApp;
        }

        private readonly bool _isShared;
        private readonly int _parentAppId;

        /// <summary>
        /// Create an attribute definition "from scratch" so for
        /// import-scenarios and code-created attribute definitions
        /// </summary>
        public ContentTypeAttribute(int appId, string name, string niceName, string type, string inputType, string notes,
            bool? visibleInEditUi, object defaultValue)
            : this(appId, name, type, false, 0, 0)
            => Metadata.Use(new List<IEntity>
            {
                AttDefBuilder.CreateAttributeMetadata(appId, niceName, notes, visibleInEditUi,
                    HelpersToRefactor.SerializeValue(defaultValue), inputType)
            });


        /// <summary>
        /// Get Attribute for specified Typ
        /// </summary>
        /// <returns><see cref="Attribute{ValueType}"/></returns>
        [PrivateApi]
        public IAttribute CreateAttribute() => CreateTypedAttribute(Name, Type);


        #region Metadata and Permissions
        /// <inheritdoc />
        public IMetadataOf Metadata
            => _metadata ?? (_metadata = !_isShared
                   ? new MetadataOf<int>(Constants.MetadataForAttribute, AttributeId, _metaOfThisApp)
                   : new MetadataOf<int>(Constants.MetadataForAttribute, AttributeId, 0, _parentAppId)
               );

        private IMetadataOf _metadata;

        /// <inheritdoc />
        [PrivateApi("because permissions will probably become an entity-based type")]
        public IEnumerable<Permission> Permissions => Metadata.Permissions;

        #endregion

    }
}
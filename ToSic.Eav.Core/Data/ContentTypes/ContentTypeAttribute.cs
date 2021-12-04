using System;
using System.Collections.Generic;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Defines an attribute with name and the type this attribute has. Part of of a <see cref="IContentType"/> definition.
    /// </summary>
    [PrivateApi("2021-09-30 changed to private, before was internal-this is just fyi, always use the interface")]
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

        /// <inheritdoc />
        /// <summary>
        /// Extended constructor when also storing the persistence Id
        /// </summary>
        // TODO: clean up call to this function, as 2 params are not used
        public ContentTypeAttribute(int appId, string name, string type, bool isTitle, int attributeId, int sortOrder, 
            IHasMetadataSource metaProvider = null, int parentApp = 0, 
            Func<IHasMetadataSource> metaSourceFinder = null) : base(name, type)
        {
            AppId = appId;
            IsTitle = isTitle;
            AttributeId = attributeId;
            SortOrder = sortOrder;
            _metaSourceFinder = metaSourceFinder;
        }

        /// <summary>
        /// Create an attribute definition "from scratch" so for
        /// import-scenarios and code-created attribute definitions
        /// </summary>
        public ContentTypeAttribute(int appId, string name, string type, List<IEntity> attributeMetadata)
            : this(appId, name, type, false, 0, 0)
            => Metadata.Use(attributeMetadata);


        /// <summary>
        /// Get Attribute for specified Typ
        /// </summary>
        /// <returns><see cref="Attribute{ValueType}"/></returns>
        [PrivateApi]
        public IAttribute CreateAttribute() => AttributeBuilder.CreateTyped(Name, Type);


        #region Metadata and Permissions
        /// <inheritdoc />
        public IMetadataOf Metadata
            => _metadata ?? (_metadata = new MetadataOf<int>((int)TargetTypes.Attribute, AttributeId, _metaSourceFinder, Name + " (" + Type + ")"));

        private IMetadataOf _metadata;
        private readonly Func<IHasMetadataSource> _metaSourceFinder;

        /// <inheritdoc />
        [PrivateApi("because permissions will probably become an entity-based type")]
        public IEnumerable<Permission> Permissions => Metadata.Permissions;

        #endregion

    }
}
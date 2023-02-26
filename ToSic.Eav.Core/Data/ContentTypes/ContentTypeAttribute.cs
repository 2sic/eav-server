using System;
using System.Collections.Generic;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;
using ToSic.Lib.Documentation;

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
        public int AttributeId { get; }

        /// <inheritdoc />
        public int SortOrder { get; }

        /// <inheritdoc />
        public bool IsTitle { get; }

        /// <inheritdoc />
        /// <summary>
        /// Extended constructor when also storing the persistence Id
        /// </summary>
        // TODO: clean up call to this function, as 2 params are not used
        public ContentTypeAttribute(int appId,
            string name,
            string type,
            bool isTitle,
            int attributeId,
            int sortOrder, 
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
        public ContentTypeAttribute(int appId, string name, string type, bool isTitle, List<IEntity> attributeMetadata)
            : this(appId, name, type, isTitle, 0, 0)
        {
            MetadataInternal.Use(attributeMetadata);
        }


        #region Metadata and Permissions

        /// <inheritdoc />
        public IMetadataOf Metadata => MetadataInternal;

        private MetadataOf<int> MetadataInternal 
            => _metadata ?? (_metadata = new MetadataOf<int>((int)TargetTypes.Attribute, AttributeId, _metaSourceFinder, Name + " (" + Type + ")"));
        private MetadataOf<int> _metadata;
        private readonly Func<IHasMetadataSource> _metaSourceFinder;

        /// <inheritdoc />
        [PrivateApi("because permissions will probably become an entity-based type")]
        public IEnumerable<Permission> Permissions => Metadata.Permissions;

        #endregion

    }
}
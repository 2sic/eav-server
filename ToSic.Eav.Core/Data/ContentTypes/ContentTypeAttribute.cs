using System;
using System.Collections.Generic;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Defines an attribute with name and the type this attribute has.
    /// Part of of a <see cref="IContentType"/> definition.
    /// </summary>
    /// <remarks>
    /// * completely #immutable since v15.04
    /// </remarks>
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

        [PrivateApi] // #SharedFieldDefinition
        public Guid? Guid { get; }

        [PrivateApi] // #SharedFieldDefinition
        public ContentTypeAttributeSysSettings SysSettings { get; }

        /// <inheritdoc />
        /// <summary>
        /// Extended constructor when also storing the persistence Id
        /// </summary>
        public ContentTypeAttribute(
            int appId,
            string name,
            ValueTypes type,
            bool isTitle,
            int attributeId,
            int sortOrder,
            Guid? guid,
            ContentTypeAttributeSysSettings sysSettings = default,
            IMetadataOf metadata = default) : base(name, type)
        {
            AppId = appId;
            IsTitle = isTitle;
            AttributeId = attributeId;
            SortOrder = sortOrder;
            Guid = guid;
            SysSettings = sysSettings;
            Metadata = metadata;
        }


        #region Metadata and Permissions

        /// <inheritdoc />
        public IMetadataOf Metadata { get; }

        /// <inheritdoc />
        [PrivateApi("because permissions will probably become an entity-based type")]
        public IEnumerable<Permission> Permissions => Metadata.Permissions;

        #endregion

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;
using static System.StringComparison;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a ContentType
    /// </summary>
    // Remarks: Before 2021-09 it was marked as PublicApi
    // We should actually make it PrivateApi, but other code references this, so we need to change that to IContentType,
    // Otherwise docs won't generate cross-links as needed
    [PrivateApi("2021-09-30 hidden now, was internal_don't use Always use the interface, not this class")]
    public partial class ContentType : IContentType, IContentTypeShared
    {
        #region simple properties

        /// <inheritdoc />
        public int AppId { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string StaticName { get; private set; }

        /// <inheritdoc />
        public string Description { get; protected set; }

        /// <inheritdoc />
        public string Scope { get; private set; }

        /// <inheritdoc />
        public int ContentTypeId { get; internal set; }

        /// <inheritdoc />
        public IList<IContentTypeAttribute> Attributes { get; set; }

        /// <inheritdoc />
        public RepositoryTypes RepositoryType { get; internal set; } = RepositoryTypes.Sql;

        /// <inheritdoc />
        public string RepositoryAddress { get; internal set; } = "";

        /// <inheritdoc />
        public bool IsDynamic { get; internal set; }

        #endregion

        /// <inheritdoc />
        public bool Is(string name) => Name.Equals(name, InvariantCultureIgnoreCase) || StaticName.Equals(name, InvariantCultureIgnoreCase);

        /// <inheritdoc />
        public IContentTypeAttribute this[string fieldName] => Attributes.FirstOrDefault(a => string.Equals(a.Name, fieldName, OrdinalIgnoreCase));


        #region New DynamicChildren Navigation - new in 12.03

        /// <inheritdoc />
        [PrivateApi("WIP 12.03")]
        // Don't cache the result, as it could change during runtime
        public string DynamicChildrenField => Metadata.GetBestValue<string>(ContentTypes.DynamicChildrenField);
        

        #endregion


        #region constructors

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new ContentType - usually when building the cache
        /// </summary>
        [PrivateApi]
        public ContentType(int appId, string name, string staticName, int attributeSetId, string scope,
            string description, 
            int? parentTypeId = null, 
            int configZoneId = 0, 
            int configAppId = 0,
            bool configurationIsOmnipresent = false,
            Func<IHasMetadataSource> metaSourceFinder = null): this(appId, name, staticName)
        {
            ContentTypeId = attributeSetId;
            Description = description;
            Scope = Scopes.RenameOldScope(scope);

            AlwaysShareConfiguration = configurationIsOmnipresent;

            if (parentTypeId != null)
                Decorators.Add(new Ancestor<IContentType>(new AppIdentity(configZoneId, configAppId),
                    parentTypeId.Value));

            // Metadata
            _metaSourceFinder = metaSourceFinder;
        }

        /// <summary>
        /// Basic initializer of ContentType class
        /// </summary>
        /// <remarks>
        /// Overload for in-memory entities
        /// </remarks>
        [PrivateApi]
        public ContentType(int appId, string name, string staticName = null)
        {
            AppId = appId;
            Name = name;
            StaticName = staticName ?? name;
        }

        #endregion




    }
}
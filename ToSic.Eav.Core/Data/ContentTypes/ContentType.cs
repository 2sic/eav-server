using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;
using static System.StringComparison;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a ContentType
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public class ContentType : IContentType, IContentTypeShared
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
        public string Scope { get; protected set; }

        /// <inheritdoc />
        public int ContentTypeId { get; }

        /// <inheritdoc />
        public IList<IContentTypeAttribute> Attributes { get; set; }

        /// <inheritdoc />
        public RepositoryTypes RepositoryType { get; internal set; } = RepositoryTypes.Sql;

        /// <inheritdoc />
        public string RepositoryAddress { get; internal set; } = "";

        /// <inheritdoc />
        public bool IsDynamic { get; internal set; }

        /// <inheritdoc />
        public bool Is(string name) => Name.Equals(name, InvariantCultureIgnoreCase) || StaticName.Equals(name, InvariantCultureIgnoreCase);

        /// <inheritdoc />
        public IContentTypeAttribute this[string fieldName] => Attributes.FirstOrDefault(a => string.Equals(a.Name, fieldName, OrdinalIgnoreCase));


        #endregion

        #region Sharing Content Types
        /// <inheritdoc />
        public int? ParentId { get; internal set; }
        /// <inheritdoc />
        public int ParentAppId { get; }
        /// <inheritdoc />
        public int ParentZoneId { get; }
        /// <inheritdoc />
        public bool AlwaysShareConfiguration { get; private set; }

        #endregion


        #region constructors

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new ContentType - usually when building the cache
        /// </summary>
        public ContentType(int appId, string name, string staticName, int attributeSetId, string scope,
            string description, int? usesConfigurationOfAttributeSet, int configZoneId, int configAppId,
            bool configurationIsOmnipresent, IHasMetadataSource metaProviderOfThisApp): this(appId, name, staticName)
        {
            ContentTypeId = attributeSetId;
            Description = description;
            Scope = scope;
            ParentId = usesConfigurationOfAttributeSet;
            ParentZoneId = configZoneId;
            ParentAppId = configAppId;
            AlwaysShareConfiguration = configurationIsOmnipresent;
            _metaOfThisApp = metaProviderOfThisApp;

        }

        /// <summary>
        /// Basic initializer of ContentType class
        /// </summary>
        /// <remarks>
        /// Overload for in-memory entities
        /// </remarks>
        public ContentType(int appId, string name, string staticName = null)
        {
            AppId = appId;
            Name = name;
            StaticName = staticName ?? name;
        }

        #endregion


        #region Helpers just for creating ContentTypes which will be imported
        [PrivateApi]
        public void SetImportParameters(string scope, string staticName, string description, bool alwaysShareDef)
        {
            Scope = scope;
            StaticName = staticName;
            Description = description;
            AlwaysShareConfiguration = alwaysShareDef;
        }

        // special values just needed for import / save 
        // todo: try to place in a sub-object to un-clutter this ContentType object
        [PrivateApi]
        public bool OnSaveSortAttributes { get; set; } = false;

        [PrivateApi]
        public string OnSaveUseParentStaticName { get; set; }


        #endregion

        #region Metadata



        /// <inheritdoc />
        public ContentTypeMetadata Metadata
            => _metadata ?? (_metadata = ParentAppId == AppId
                ? new ContentTypeMetadata(StaticName, _metaOfThisApp)
                : new ContentTypeMetadata(StaticName, ParentZoneId, ParentAppId));
        private ContentTypeMetadata _metadata;
        private readonly IHasMetadataSource _metaOfThisApp;

        IMetadataOf IHasMetadata.Metadata => Metadata;
        #endregion

    }
}
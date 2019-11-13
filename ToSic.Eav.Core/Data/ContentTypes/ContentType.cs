using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a ContentType
    /// </summary>
    [PublicApi]
    public class ContentType : IContentType, IContentTypeShared, IHasExternalI18n
    {
        #region simple properties

        /// <inheritdoc />
        public int AppId { get; }

        /// <inheritdoc />
        public string Name { get; protected set; }

        /// <inheritdoc />
        public string StaticName { get; protected set; }

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
        public bool Is(string name) => Name == name || StaticName == name;

        /// <inheritdoc />
        public IContentTypeAttribute this[string fieldName] => Attributes.FirstOrDefault(a => a.Name == fieldName);


        #endregion

        #region Sharing Content Types
        /// <inheritdoc />
        public int? ParentId { get; internal set; }
        /// <inheritdoc />
        public int ParentAppId { get; }
        /// <inheritdoc />
        public int ParentZoneId { get; }
        /// <inheritdoc />
        public bool AlwaysShareConfiguration { get; protected set; }

        #endregion


        #region constructors

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new ContentType - usually when building the cache
        /// </summary>
        public ContentType(int appId, string name, string staticName, int attributeSetId, string scope,
            string description, int? usesConfigurationOfAttributeSet, int configZoneId, int configAppId,
            bool configurationIsOmnipresent, IDeferredEntitiesList metaProviderOfThisApp): this(appId, name, staticName)
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
                   ? new ContentTypeMetadata(StaticName,
                       _metaOfThisApp)
                       : new ContentTypeMetadata(StaticName, ParentZoneId, ParentAppId));
        private ContentTypeMetadata _metadata;
        private readonly IDeferredEntitiesList _metaOfThisApp;

        #endregion

        #region external i18n
        [PrivateApi]
        public string I18nKey { get; protected set; } = null;
        #endregion

    }


}
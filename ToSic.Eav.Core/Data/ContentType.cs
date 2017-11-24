using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Data
{
    /// <inheritdoc cref="IContentType" />
    /// <summary>
    /// Represents a Content Type
    /// </summary>
    public class ContentType : IContentType, IUsesSharedDefinition, IHasExternalI18n
    {
        #region simple properties

        public int AppId { get; }
        public string Name { get; protected set; }
        public string StaticName { get; protected set; }
        public string Description { get; protected set; }
        public string Scope { get; protected set; }
        public int ContentTypeId { get; }
        public IList<IAttributeDefinition> Attributes { get; set; }
        //public bool IsInstalledInPrimaryStorage { get; protected set; } = true;
        public RepositoryTypes RepositoryType { get; internal set; } = RepositoryTypes.Sql;

        public string RepositoryAddress { get; internal set; } = "";

        public bool IsDynamic { get; internal set; }

        /// <inheritdoc />
        public IAttributeDefinition this[string fieldName] => Attributes.FirstOrDefault(a => a.Name == fieldName);


        #endregion

        #region Sharing Content Types
        public int? ParentId { get; internal set; }
        public int ParentAppId { get; }
        public int ParentZoneId { get; }
        public bool AlwaysShareConfiguration { get; protected set; }

        #endregion


        #region constructors

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the ContentType class.
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

        public void SetImportParameters(string scope, string staticName, string description, bool alwaysShareDef)
        {
            Scope = scope;
            StaticName = staticName;
            Description = description;
            AlwaysShareConfiguration = alwaysShareDef;
        }

        // special values just needed for import / save 
        // todo: try to place in a sub-object to un-clutter this ContentType object
        public bool OnSaveSortAttributes { get; set; } = false;

        public string OnSaveUseParentStaticName { get; set; }


        #endregion

        #region Metadata

        public IMetadataOfItem Metadata
            => _metadata ?? (_metadata = ParentAppId == AppId
                   ? new OfMetadataOfItem<string>(Constants.MetadataForContentType, StaticName, _metaOfThisApp)
                   : new OfMetadataOfItem<string>(Constants.MetadataForContentType, StaticName, ParentZoneId, ParentAppId)
               );
        private IMetadataOfItem _metadata;
        private readonly IDeferredEntitiesList _metaOfThisApp;

        #endregion

        #region external i18n
        public string I18nKey { get; protected set; } = null;
        #endregion
    }
}
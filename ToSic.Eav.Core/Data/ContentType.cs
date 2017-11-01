using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <inheritdoc cref="IContentType" />
    /// <summary>
    /// Represents a Content Type
    /// </summary>
    public class ContentType : IContentType, IUsesSharedDefinition, IHasExternalI18n
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
        public IList<IAttributeDefinition> Attributes { get; set; }

        /// <inheritdoc />
        public IAttributeDefinition this[string fieldName] => Attributes.FirstOrDefault(a => a.Name == fieldName);

        public bool IsInstalledInPrimaryStorage { get; protected set; } = true;

        public bool IsDynamic { get; internal set; }

        #endregion

        #region Sharing Content Types

        /// <inheritdoc />
        public int? ParentId { get; protected set; }

        /// <inheritdoc />
        public int ParentAppId { get; }

        /// <inheritdoc />
        public int ParentZoneId { get; }

        /// <inheritdoc />
        public bool AlwaysShareConfiguration { get; protected set; }

        #endregion


        #region constructors

        /// <summary>
        /// Initializes a new instance of the ContentType class.
        /// </summary>
        public ContentType(int appId, string name, string staticName, int attributeSetId, string scope,
            string description, int? usesConfigurationOfAttributeSet, int configZoneId, int configAppId,
            bool configurationIsOmnipresent, IDeferredEntitiesList metaProviderOfThisApp)
        {
            AppId = appId;
            Name = name;
            StaticName = staticName;
            ContentTypeId = attributeSetId;
            Description = description;
            Scope = scope;
            ParentId = usesConfigurationOfAttributeSet;
            ParentZoneId = configZoneId;
            ParentAppId = configAppId;
            AlwaysShareConfiguration = configurationIsOmnipresent;
            //_appMetadataProvider = metaProvider;
            _metaProviderOfThisApp = metaProviderOfThisApp;

        }

        /// <summary>
        /// Overload for in-memory entities
        /// </summary>
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

        #region Metadata TODO - TRYING TO MOVE INTO A SEPARATE SUB OBJECT FOR BETTER RE-USE

        // The metadata is either from the same app, or from a remote app
        public IItemMetadata Metadata => _metadata ?? (_metadata =
                                             ParentAppId == AppId
                                                 ? new MetadataOfItem<string>(Constants.MetadataForContentType,
                                                     StaticName, _metaProviderOfThisApp)
                                                 : new MetadataOfItem<string>(Constants.MetadataForContentType,
                                                     StaticName, ParentZoneId, ParentAppId)
                                         );
        private IItemMetadata _metadata;
        private readonly IDeferredEntitiesList _metaProviderOfThisApp;

        #endregion

        #region external i18n
        public string I18nKey { get; protected set; } = null;
        #endregion
    }
}
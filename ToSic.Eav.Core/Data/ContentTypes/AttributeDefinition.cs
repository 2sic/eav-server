using System.Collections.Generic;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data
{
    /// <inheritdoc cref="AttributeBase" />
    /// <summary>
    /// Represents an Attribute with Values of a Generic Type
    /// </summary>
    public class AttributeDefinition : AttributeBase, IAttributeDefinition
    {
        public int AppId { get; }
        // additional info for the persistence layer
        public int AttributeId { get; set; }

        public int SortOrder { get; internal set; }

        public bool IsTitle { get; set; }

        private readonly IDeferredEntitiesList _metaOfThisApp;

        /// <inheritdoc />
        /// <summary>
        /// Extended constructor when also storing the persistance ID-Info
        /// </summary>
        public AttributeDefinition(int appId, string name, string type, bool isTitle, int attributeId, int sortOrder, IDeferredEntitiesList metaProvider = null, int parentApp = 0/*, int parentId = 0*/) : base(name, type)
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
        // ReSharper disable once InheritdocConsiderUsage
        public AttributeDefinition(int appId, string name, string niceName, string type, string inputType, string notes,
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
        public IAttribute CreateAttribute() => CreateTypedAttribute(Name, Type);


        #region Metadata and Permissions
        public IMetadataOfItem Metadata
            => _metadata ?? (_metadata = !_isShared
                   ? new MetadataOf<int>(Constants.MetadataForAttribute, AttributeId, _metaOfThisApp)
                   : new MetadataOf<int>(Constants.MetadataForAttribute, AttributeId, 0, _parentAppId)
               );

        private IMetadataOfItem _metadata;

        public IEnumerable<IEntity> Permissions => Metadata.Permissions;

        #endregion

        #region InputType

        public string InputType => FindInputType();

        public string InputTypeTempBetterForNewUi => FindInputType2();

        /// <summary>
        /// The old method, which returns the text "unknown" if not known. 
        /// As soon as the new UI is used, this must be removed / deprecated
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// It's important to NOT cache this result, because it can change during runtime, and then a cached info would be wrong. 
        /// </remarks>
        private string FindInputType()
        {
            var inputType = Metadata.GetBestValue<string>(Constants.MetadataFieldAllInputType, Constants.MetadataFieldTypeAll);

            return string.IsNullOrEmpty(inputType) 
                ? "unknown" // unknown will let the UI fallback on other mechanisms
                : inputType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// It's important to NOT cache this result, because it can change during runtime, and then a cached info would be wrong. 
        /// </remarks>
        private string FindInputType2()
        {
            // Preferred storage and available in all fields defined after 2sxc ca. 6 or 7
            var inputType = Metadata.GetBestValue<string>(
                Constants.MetadataFieldAllInputType, Constants.MetadataFieldTypeAll);

            // if not available, check older metadata, where it was on the @String
            if (string.IsNullOrWhiteSpace(inputType))
            {
                inputType = Metadata.GetBestValue<string>(
                    Constants.MetadataFieldAllInputType, Constants.MetadataFieldTypeString);
                // if found, check and maybe add prefix string
                const string prefix = "string-";
                if (!string.IsNullOrWhiteSpace(inputType) && !inputType.StartsWith(prefix))
                    inputType = prefix + inputType;
            }

            // if still not found, assemble from known type
            if (string.IsNullOrWhiteSpace(inputType))
                inputType = Type.ToLowerInvariant() + "-default";

            return inputType;
        }

        #endregion InputType
    }
}
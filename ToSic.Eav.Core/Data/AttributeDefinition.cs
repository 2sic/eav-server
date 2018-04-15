using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <inheritdoc cref="AttributeBase" />
    /// <summary>
    /// Represents an Attribute with Values of a Generic Type
    /// </summary>
    public partial class AttributeDefinition : AttributeBase, IAttributeDefinition
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


        #region material for defining/creating attributes / defining them for import
        public IMetadataOfItem Metadata
            => _metadata ?? (_metadata = !_isShared
                   ? new MetadataOf<int>(Constants.MetadataForAttribute, AttributeId, _metaOfThisApp)
                   : new MetadataOf<int>(Constants.MetadataForAttribute, AttributeId, 0, _parentAppId)
               );

        private IMetadataOfItem _metadata;

        #endregion

        #region InputType
        public string InputType => _inputType ?? (_inputType = FindInputType());
        private string _inputType;

        private string FindInputType()
        {
            var inputType = Metadata
                .FirstOrDefault(d => d.Type.StaticName == "@All")
                ?.GetBestValue("InputType");

            return string.IsNullOrEmpty(inputType as string) ? Type.ToLower() + "-default" /*"unknown"*/ : inputType.ToString();
        }

        #endregion InputType
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Enums;
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

        private readonly IDeferredEntitiesList _appMetadataProvider;

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
            _appMetadataProvider = metaProvider;
            _isShared = parentApp != 0;
            _parentAppId = parentApp;
        }

        private readonly bool _isShared;
        private readonly int _parentAppId;

        /// <inheritdoc />
        public AttributeDefinition(int appId, string name, string niceName, AttributeTypeEnum type, string inputType, string notes, bool? visibleInEditUi, object defaultValue) 
            : this(appId, name, niceName, type.ToString(), inputType, notes, visibleInEditUi, defaultValue) { }

        /// <summary>
        /// Create an attribute definition "from scratch" so for
        /// import-scenarios and code-created attribute definitions
        /// </summary>
        // ReSharper disable once InheritdocConsiderUsage
        public AttributeDefinition(int appId, string name, string niceName, string type, string inputType, string notes, bool? visibleInEditUi, object defaultValue): this(appId, name, type, false, 0, 0)
        {
            _metaItems = new List<IEntity> { AttDefBuilder.CreateAttributeMetadata(appId, niceName, notes, visibleInEditUi, HelpersToRefactor.SerializeValue(defaultValue), inputType) };
        }



        /// <summary>
        /// Get Attribute for specified Typ
        /// </summary>
        /// <returns><see cref="Attribute{ValueType}"/></returns>
        public IAttribute CreateAttribute() => CreateTypedAttribute(Name, Type);


        #region material for defining/creating attributes / defining them for import

        public List<IEntity> MetadataItems
        {
            get
            {
                if (_metaItems != null) return _metaItems;

                var metadataProvider = _isShared
                    ? Factory.Resolve<IRemoteMetadataProvider>()?.OfApp(_parentAppId)
                    : _appMetadataProvider?.Metadata;

                _metaItems = metadataProvider?.GetMetadata(
                             Constants.MetadataForAttribute, AttributeId).ToList()
                         ?? new List<IEntity>();

                return _metaItems;
            }
            internal set => _metaItems = value;
        }
        // ReSharper disable once InconsistentNaming
        private List<IEntity> _metaItems;

        public bool HasMetadata => _metaItems != null && _metaItems.Any();

        public void AddMetadata(string type, Dictionary<string, object> values)
            => MetadataItems.Add(new Entity(AppId, Guid.Empty, type, values));

        #endregion
        
    }
}
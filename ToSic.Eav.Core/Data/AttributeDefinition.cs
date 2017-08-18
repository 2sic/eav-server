using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
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

        private IDeferredEntitiesList metadataSource;

        /// <summary>
        /// Extended constructor when also storing the persistance ID-Info
        /// </summary>
        public AttributeDefinition(int appId, string name, string type, bool isTitle, int attributeId, int sortOrder, IDeferredEntitiesList metaSource = null) : base(name, type)
        {
            AppId = appId;
            IsTitle = isTitle;
            AttributeId = attributeId;
            SortOrder = sortOrder;
            metadataSource = metaSource;
        }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public AttributeDefinition(int appId, string name, string niceName, string type, string notes, bool? visibleInEditUi, object defaultValue): this(appId, name, type, false, 0, 0)
        {
            _items = new List<IEntity> { AttDefBuilder.CreateAttributeMetadata(appId, niceName, notes, visibleInEditUi, HelpersToRefactor.SerializeValue(defaultValue)) };
        }



        /// <summary>
        /// Get Attribute for specified Typ
        /// </summary>
        /// <returns><see cref="Attribute{ValueType}"/></returns>
        public IAttribute CreateAttribute() => CreateTypedAttribute(Name, Type);


        #region material for defining/creating attributes / defining them for import

        public List<IEntity> Items => _items ?? (_items = metadataSource?.Metadata.GetMetadata(Constants.MetadataForAttribute, AttributeId).ToList() ?? new List<IEntity>());

        internal List<IEntity> _items;

        public bool HasMetadata => _items != null && _items.Any();


        #endregion
    }
}
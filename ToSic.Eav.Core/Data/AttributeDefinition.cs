using System;
using System.Collections.Generic;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents an Attribute with Values of a Generic Type
    /// </summary>
    public class AttributeDefinition : AttributeBase, IAttributeDefinition
    {
        // additional info for the persistence layer
        public int AttributeId { get; set; }

        public int SortOrder { get; internal set; }

        public bool IsTitle { get; set; }

        /// <summary>
        /// Extended constructor when also storing the persistance ID-Info
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="isTitle"></param>
        /// <param name="attributeId"></param>
        /// <param name="sortOrder"></param>
        public AttributeDefinition(string name, string type, bool isTitle, int attributeId, int sortOrder): base(name, type/*, isTitle*/)
		{
            IsTitle = isTitle;
            AttributeId = attributeId;
            SortOrder = sortOrder;
		}

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public AttributeDefinition(string name, string niceName, string type, string notes, bool? visibleInEditUi, object defaultValue): this(name, type, false, 0, 0)
        {
            InternalAttributeMetaData = new List<Entity> { Builder.AttributeDefinition.CreateAttributeMetadata(niceName, notes, visibleInEditUi, HelpersToRefactor.SerializeValue(defaultValue)) };
        }



        /// <summary>
        /// Get Attribute for specified Typ
        /// </summary>
        /// <returns><see cref="Attribute{ValueType}"/></returns>
        public IAttribute CreateAttribute() => CreateTypedAttribute(Name, Type);


        #region material for defining/creating attributes / defining them for import

        public List<Entity> InternalAttributeMetaData { get; set; }
        #endregion
    }
}
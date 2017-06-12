using System;
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
        /// Get Attribute for specified Typ
        /// </summary>
		/// <returns><see cref="Attribute{ValueType}"/></returns>
        public IAttribute CreateAttribute()
        {
            switch (ControlledType)
            {
                case AttributeTypeEnum.Boolean:
                    return new Attribute<bool?>(Name, Type);
                case AttributeTypeEnum.DateTime:
                    return new Attribute<DateTime?>(Name, Type);
                case AttributeTypeEnum.Number:
                    return new Attribute<decimal?>(Name, Type);
                case AttributeTypeEnum.Entity:
                    return new Attribute<EntityRelationship>(Name, Type) { Values = new IValue[] { Value.NullRelationship} };
                case AttributeTypeEnum.String:
                case AttributeTypeEnum.Hyperlink:
                case AttributeTypeEnum.Custom:
                default:
                    return new Attribute<string>(Name, Type);
            }
        }

    }
}
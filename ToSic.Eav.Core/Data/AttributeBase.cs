using System;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents an Attribute with Values of a Generic Type
    /// </summary>
    public class AttributeBase : IAttributeBase
    {
        public string Name { get; set; }
        public string Type { get; set; }

        private AttributeTypeEnum _controlledType = AttributeTypeEnum.Undefined;

        public AttributeTypeEnum ControlledType
        {
            get
            {
                // if the type has not been set yet, try to look it up...
                if (_controlledType == AttributeTypeEnum.Undefined
                    && Type != null
                    && Enum.IsDefined(typeof (AttributeTypeEnum), Type))
                    _controlledType = (AttributeTypeEnum) Enum.Parse(typeof (AttributeTypeEnum), Type);
                return _controlledType;
            }
            internal set { _controlledType = value; }
        }

        public bool IsTitle { get; set; }

        // 2017-06-07 2dm moving to AttributeDefinition
        // additional info for the persistence layer
        //public int AttributeId { get; set; }

        //public int SortOrder { get; internal set; }

        /// <summary>
        /// Extended constructor when also storing the persistance ID-Info
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="isTitle"></param>
        ///// <param name="attributeId"></param>
        ///// <param name="sortOrder"></param>
        public AttributeBase(string name, string type, bool isTitle)//, int attributeId, int sortOrder)
		{
            Name = name;
            Type = type;
            IsTitle = isTitle;

            //AttributeId = attributeId;
            //SortOrder = sortOrder;
		}

    }
}
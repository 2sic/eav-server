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
        public AttributeDefinition(string name, string type, bool isTitle, int attributeId, int sortOrder): base(name, type, isTitle)
		{
            IsTitle = isTitle;
            AttributeId = attributeId;
            SortOrder = sortOrder;
		}

    }
}
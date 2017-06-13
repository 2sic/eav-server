using System;
using System.Collections.Generic;

namespace ToSic.Eav.Data.Builder
{
    public static class AttributeDefinition
    {

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static Data.AttributeDefinition BooleanAttribute(string staticName, string name, string notes, bool? visibleInEditUi, bool? defaultValue = null)
        {
            var attribute = new Data.AttributeDefinition(staticName, name, AttributeTypeEnum.Boolean.ToString(), notes, visibleInEditUi, defaultValue);
            return attribute;
        }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static Data.AttributeDefinition StringAttribute(string staticName, string niceName, string notes, bool? visibleInEditUi, string inputType = null, int? rowCount = null, string defaultValue = null)
        {
            var attribute = new Data.AttributeDefinition(staticName, niceName, AttributeTypeEnum.String.ToString(), notes, visibleInEditUi, defaultValue);
            attribute.InternalAttributeMetaData.Add(CreateStringAttribMetadata(inputType, rowCount));
            return attribute;
        }

        /// <summary>
        /// Shortcut to get an @All Entity Describing an Attribute
        /// </summary>
        public static Entity CreateAttributeMetadata(string name, string notes, bool? visibleInEditUi, string defaultValue)
        {
            var valDic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(name)) valDic.Add("Name", name);
            if (!string.IsNullOrEmpty(notes)) valDic.Add("Notes", notes);
            if (visibleInEditUi.HasValue) valDic.Add("VisibleInEditUI", visibleInEditUi);
            if (defaultValue != null) valDic.Add("DefaultValue", defaultValue);

            return new Entity(Guid.Empty, "@All", valDic);
        }

        public static Entity CreateStringAttribMetadata(string inputType, int? rowCount)
        {
            var valDic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(inputType)) valDic.Add("InputType", inputType);
            if (rowCount.HasValue) valDic.Add("RowCount", rowCount);

            return new Entity(Guid.Empty, "@String", valDic);

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Enums;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data.Builder
{
    public static class AttDefBuilder
    {

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static AttributeDefinition BooleanAttribute(int appId, string staticName, string name, string notes, bool? visibleInEditUi, bool? defaultValue = null)
        {
            var attribute = new AttributeDefinition(appId, staticName, name, AttributeTypeEnum.Boolean.ToString(), notes, visibleInEditUi, defaultValue);
            return attribute;
        }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static AttributeDefinition StringAttribute(int appId, string staticName, string niceName, string notes, bool? visibleInEditUi, string inputType = null, int? rowCount = null, string defaultValue = null)
        {
            var attribute = new AttributeDefinition(appId, staticName, niceName, AttributeTypeEnum.String.ToString(), notes, visibleInEditUi, defaultValue);
            attribute.Items.Add(CreateStringAttribMetadata(appId, inputType, rowCount));
            return attribute;
        }

        /// <summary>
        /// Shortcut to get an @All Entity Describing an Attribute
        /// </summary>
        public static Entity CreateAttributeMetadata(int appId, string name, string notes, bool? visibleInEditUi, string defaultValue)
        {
            var valDic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(name)) valDic.Add("Name", name);
            if (!string.IsNullOrEmpty(notes)) valDic.Add("Notes", notes);
            if (visibleInEditUi.HasValue) valDic.Add("VisibleInEditUI", visibleInEditUi);
            if (defaultValue != null) valDic.Add("DefaultValue", defaultValue);

            return new Entity(appId, Guid.Empty, "@All", valDic);
        }

        public static Entity CreateStringAttribMetadata(int appId, string inputType, int? rowCount)
        {
            var valDic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(inputType)) valDic.Add("InputType", inputType);
            if (rowCount.HasValue) valDic.Add("RowCount", rowCount);

            return new Entity(appId, Guid.Empty, "@String", valDic);

        }

        public static int Id(this IContentType ct, string name)
        {
            return
                ct.Attributes.First(a => string.Equals(a.Name, name, StringComparison.InvariantCultureIgnoreCase))
                    .AttributeId;
        }

        public static void SetSortOrder(this AttributeDefinition attDef, int sortOrder) => attDef.SortOrder = sortOrder;

        public static void AddItems(this AttributeDefinition attDef, List<IEntity> items) => attDef._items = items;

    }
}

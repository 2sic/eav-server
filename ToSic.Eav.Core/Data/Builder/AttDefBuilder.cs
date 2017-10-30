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
            var attribute = new AttributeDefinition(appId, staticName, name, AttributeTypeEnum.Boolean, null, notes, visibleInEditUi, defaultValue);
            return attribute;
        }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static AttributeDefinition StringAttribute(int appId, string staticName, string niceName, string notes, bool? visibleInEditUi, string inputType = null, int? rowCount = null, string defaultValue = null)
        {
            // todo 2017-10-07 2dm must test, added input-type here! might affect upgrades!
            var attribute = new AttributeDefinition(appId, staticName, niceName, AttributeTypeEnum.String, inputType, notes, visibleInEditUi, defaultValue);
            attribute.MetadataItems.Add(CreateV7And8StringAttribMetadata(appId, inputType, rowCount));
            return attribute;
        }

        /// <summary>
        /// Shortcut to get an @All Entity Describing an Attribute
        /// </summary>
        public static Entity CreateAttributeMetadata(int appId, string name, string notes, bool? visibleInEditUi, string defaultValue, string inputType)
        {
            var valDic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(name)) valDic.Add("Name", name);
            if (!string.IsNullOrEmpty(notes)) valDic.Add("Notes", notes);
            if (visibleInEditUi.HasValue) valDic.Add("VisibleInEditUI", visibleInEditUi);
            if (defaultValue != null) valDic.Add("DefaultValue", defaultValue);
            if (!string.IsNullOrEmpty(inputType)) valDic.Add("InputType", inputType);

            return new Entity(appId, Guid.Empty, "@All", valDic);
        }

        public static Entity CreateV7And8StringAttribMetadata(int appId, string inputType, int? rowCount)
        {
            var valDic = new Dictionary<string, object>();
            // todo 2017-10-07 this is probably wrong, as the input-type is on the other attribute...
            // ... but it could also be right, as it's currently only used in upgrade routines on v7 & v8
            // ...and in 7/8 this used to be the place to set this information
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

        public static void AddMetadata(this AttributeDefinition attDef, List<IEntity> items) => attDef.MetadataItems = items;

    }
}

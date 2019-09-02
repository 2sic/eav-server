using System;
using System.Collections.Generic;
using ToSic.Eav.Types;

namespace ToSic.Eav.Data.Builder
{
    public static class AttDefBuilder
    {
        
        /// <summary>
        /// Shortcut to get an @All Entity Describing an Attribute
        /// </summary>
        internal static Entity CreateAttributeMetadata(int appId, string name, string notes, bool? visibleInEditUi, string defaultValue, string inputType)
        {
            var valDic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(name)) valDic.Add("Name", name);
            if (!string.IsNullOrEmpty(notes)) valDic.Add("Notes", notes);
            if (visibleInEditUi.HasValue) valDic.Add("VisibleInEditUI", visibleInEditUi);
            if (defaultValue != null) valDic.Add("DefaultValue", defaultValue);
            if (!string.IsNullOrEmpty(inputType)) valDic.Add(Constants.MetadataFieldAllInputType, inputType);

            return new Entity(appId, Guid.Empty, Global.FindContentType(Constants.MetadataFieldTypeAll), valDic);
        }


        public static void SetSortOrder(this AttributeDefinition attDef, int sortOrder) => attDef.SortOrder = sortOrder;

    }
}

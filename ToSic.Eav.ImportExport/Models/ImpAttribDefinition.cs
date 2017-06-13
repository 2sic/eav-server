using System;
using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.ImportExport.Models
{
    public class ImpAttribDefinition: AttributeDefinition
    {
        public List<Entity> AttributeMetaData { get; set; }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public ImpAttribDefinition(string name, string niceName, string type, string notes, bool? visibleInEditUi, object defaultValue): base(name, type, false, 0, 0)
        {
            AttributeMetaData = new List<Entity> { CreateAttributeMetadata(niceName, notes, visibleInEditUi, HelpersToRefactor.SerializeValue(defaultValue)) };
        }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static ImpAttribDefinition StringAttribute(string staticName, string niceName, string notes, bool? visibleInEditUi, string inputType = null, int? rowCount = null, string defaultValue = null)
        {
            var attribute = new ImpAttribDefinition(staticName, niceName, AttributeTypeEnum.String.ToString(), notes, visibleInEditUi, defaultValue);
            attribute.AttributeMetaData.Add(CreateStringAttribMetadata(inputType, rowCount));
            return attribute;
        }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static ImpAttribDefinition BooleanAttribute(string staticName, string name, string notes, bool? visibleInEditUi, bool? defaultValue = null)
        {
            var attribute = new ImpAttribDefinition(staticName, name, AttributeTypeEnum.Boolean.ToString(), notes, visibleInEditUi, defaultValue);
            return attribute;
        }

        /// <summary>
        /// Shortcut to get an @All Entity Describing an Attribute
        /// </summary>
        private static Entity CreateAttributeMetadata(string name, string notes, bool? visibleInEditUi, string defaultValue)
        {
            var valDic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(name))
                valDic.Add("Name", name);
            if (!string.IsNullOrEmpty(notes))
                valDic.Add("Notes", notes);
            if (visibleInEditUi.HasValue)
                valDic.Add("VisibleInEditUI", visibleInEditUi);
            if (defaultValue != null)
                valDic.Add("DefaultValue", defaultValue);

            return new Entity(Guid.Empty, "@All", valDic);
        }

        private static Entity CreateStringAttribMetadata(string inputType, int? rowCount)
        {
            var valDic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(inputType)) valDic.Add("InputType", inputType);
            if (rowCount.HasValue) valDic.Add("RowCount", rowCount);

            return new Entity(Guid.Empty, "@String", valDic);

        }
    }
}
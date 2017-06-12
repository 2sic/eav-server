using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.ImportExport.Models
{
    public class ImpAttribDefinition: AttributeDefinition
    {
        public List<ImpEntity> AttributeMetaData { get; set; }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public ImpAttribDefinition(string name, string niceName, string type, string notes, bool? visibleInEditUi, object defaultValue): base(name, type, false, 0, 0)
        {
            AttributeMetaData = new List<ImpEntity> { CreateAttributeMetadata(niceName, notes, visibleInEditUi, HelpersToRefactor.SerializeValue(defaultValue)) };
        }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static ImpAttribDefinition StringAttribute(string staticName, string niceName, string notes, bool? visibleInEditUi, string inputType = null, int? rowCount = null, string defaultValue = null)
        {
            var attribute = new ImpAttribDefinition(staticName, niceName, AttributeTypeEnum.String.ToString(), notes, visibleInEditUi, defaultValue);
            attribute.AttributeMetaData.Add(attribute.CreateStringAttribMetadata(inputType, rowCount));
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
        private ImpEntity CreateAttributeMetadata(string name, string notes, bool? visibleInEditUi, string defaultValue)
        {
            //var allEntity = new ImpEntity("@All")
            //{
            //    ImpAttributes = new Dictionary<string, List<IValue>>()
            //};
            //if (!string.IsNullOrEmpty(name))
            //    allEntity.ImpAttributes.Add("Name", new List<IValue> { new Value<string>(name ) });
            //if (!string.IsNullOrEmpty(notes))
            //    allEntity.ImpAttributes.Add("Notes", new List<IValue> { new Value<string>(notes ) });
            //if (visibleInEditUi.HasValue)
            //    allEntity.ImpAttributes.Add("VisibleInEditUI", new List<IValue> { new Value<bool?>(visibleInEditUi ) });
            //if (defaultValue != null)
            //    allEntity.ImpAttributes.Add("DefaultValue", new List<IValue> { new Value<string>(defaultValue ) });

            //return allEntity;

            // 2017-06-12 new using entity
            var valDic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(name))
                valDic.Add("Name", name);
            if (!string.IsNullOrEmpty(notes))
                valDic.Add("Notes", notes);
            if (visibleInEditUi.HasValue)
                valDic.Add("VisibleInEditUI", visibleInEditUi);
            if (defaultValue != null)
                valDic.Add("DefaultValue", defaultValue);

            return new ImpEntity(Guid.Empty, "@All", valDic, "");
        }

        private ImpEntity CreateStringAttribMetadata(string inputType, int? rowCount)
        {
            // old
            //var Values = new Dictionary<string, List<IValue>>();
            //if (!string.IsNullOrEmpty(inputType))
            //    Values.Add("InputType", new List<IValue> { new Value<string>(inputType) });
            //if (rowCount.HasValue)
            //    Values.Add("RowCount", new List<IValue> { new Value<decimal?>(rowCount) });
            //var stringEntity = new ImpEntity("@String")
            //{
            //    ImpAttributes = Values
            //};
            //return stringEntity;

            var valDic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(inputType)) valDic.Add("InputType", inputType);
            if (rowCount.HasValue) valDic.Add("RowCount", rowCount);

            // 2017-06-12 new using entity
            return new ImpEntity(Guid.Empty, "@String", valDic, "");

        }
    }
}
﻿using System.Collections.Generic;
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
        private ImpEntity CreateAttributeMetadata(string name, string notes, bool? visibleInEditUi, string defaultValue = null)
        {
            var allEntity = new ImpEntity("@All")
            {
                //AttributeSetStaticName = "@All",
                Values = new Dictionary<string, List<IValue>>()
            };
            if (!string.IsNullOrEmpty(name))
                allEntity.Values.Add("Name", new List<IValue> { new Value<string>(/*allEntity,*/ name ) });
            if (!string.IsNullOrEmpty(notes))
                allEntity.Values.Add("Notes", new List<IValue> { new Value<string>(/*allEntity,*/ notes ) });
            if (visibleInEditUi.HasValue)
                allEntity.Values.Add("VisibleInEditUI", new List<IValue> { new Value<bool?>(/*allEntity,*/ visibleInEditUi ) });
            if (defaultValue != null)
                allEntity.Values.Add("DefaultValue", new List<IValue> { new Value<string>(/*allEntity,*/ defaultValue ) });

            return allEntity;
        }

        private ImpEntity CreateStringAttribMetadata(string inputType, int? rowCount)
        {
            var stringEntity = new ImpEntity("@String")
            {
                //AttributeSetStaticName = "@String",
                Values = new Dictionary<string, List<IValue>>()
            };
            if (!string.IsNullOrEmpty(inputType))
                stringEntity.Values.Add("InputType", new List<IValue> { new Value<string>(/*stringEntity,*/ inputType ) });
            if (rowCount.HasValue)
                stringEntity.Values.Add("RowCount", new List<IValue> { new Value<decimal?>(/*stringEntity,*/ rowCount ) });

            return stringEntity;
        }
    }
}
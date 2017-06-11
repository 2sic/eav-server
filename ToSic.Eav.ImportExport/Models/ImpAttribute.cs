using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Interfaces;

namespace ToSic.Eav.ImportExport.Models
{
    public class ImpAttribute//: AttributeDefinition
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public string InputType { get; set; }
        public List<ImpEntity> AttributeMetaData { get; set; }

        // 2017-06-11 removing empty contructor
        ///// <summary>
        ///// Default Constructor
        ///// </summary>
        //public ImpAttribute() { }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public ImpAttribute(string name, string niceName, string type, string notes, bool? visibleInEditUi, object defaultValue)
        {
            Name = name;
            Type = type;//.ToString();
            AttributeMetaData = new List<ImpEntity> { GetAttributeMetaData(niceName, notes, visibleInEditUi, HelpersToRefactor.SerializeValue(defaultValue)) };
        }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static ImpAttribute StringAttribute(string staticName, string niceName, string notes, bool? visibleInEditUi, string inputType = null, int? rowCount = null, string defaultValue = null)
        {
            var attribute = new ImpAttribute(staticName, niceName, AttributeTypeEnum.String.ToString(), notes, visibleInEditUi, defaultValue);
            attribute.AttributeMetaData.Add(attribute.GetStringAttributeMetaData(inputType, rowCount));
            return attribute;
        }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static ImpAttribute BooleanAttribute(string staticName, string name, string notes, bool? visibleInEditUi, bool? defaultValue = null)
        {
            var attribute = new ImpAttribute(staticName, name, AttributeTypeEnum.Boolean.ToString(), notes, visibleInEditUi, defaultValue);
            return attribute;
        }

        /// <summary>
        /// Shortcut to get an @All Entity Describing an Attribute
        /// </summary>
        private ImpEntity GetAttributeMetaData(string name, string notes, bool? visibleInEditUi, string defaultValue = null)
        {
            var allEntity = new ImpEntity
            {
                AttributeSetStaticName = "@All",
                Values = new Dictionary<string, List<IImpValue>>()
            };
            if (!string.IsNullOrEmpty(name))
                allEntity.Values.Add("Name", new List<IImpValue> { new ImpValue<string>(allEntity, name ) });
            if (!string.IsNullOrEmpty(notes))
                allEntity.Values.Add("Notes", new List<IImpValue> { new ImpValue<string>(allEntity, notes ) });
            if (visibleInEditUi.HasValue)
                allEntity.Values.Add("VisibleInEditUI", new List<IImpValue> { new ImpValue<bool?>(allEntity, visibleInEditUi ) });
            if (defaultValue != null)
                allEntity.Values.Add("DefaultValue", new List<IImpValue> { new ImpValue<string>(allEntity, defaultValue ) });

            return allEntity;
        }

        private ImpEntity GetStringAttributeMetaData(string inputType, int? rowCount)
        {
            var stringEntity = new ImpEntity
            {
                AttributeSetStaticName = "@String",
                Values = new Dictionary<string, List<IImpValue>>()
            };
            if (!string.IsNullOrEmpty(inputType))
                stringEntity.Values.Add("InputType", new List<IImpValue> { new ImpValue<string>(stringEntity, inputType ) });
            if (rowCount.HasValue)
                stringEntity.Values.Add("RowCount", new List<IImpValue> { new ImpValue<decimal?>(stringEntity, rowCount ) });

            return stringEntity;
        }
    }
}
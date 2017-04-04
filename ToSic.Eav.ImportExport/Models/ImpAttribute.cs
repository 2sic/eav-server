using System.Collections.Generic;
using ToSic.Eav.Import;
using ToSic.Eav.ImportExport.Interfaces;

namespace ToSic.Eav.ImportExport.Models
{
    public class ImpAttribute
    {
        public string StaticName { get; set; }
        public string Type { get; set; }

        public string InputType { get; set; }
        public List<ImpEntity> AttributeMetaData { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ImpAttribute() { }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        internal ImpAttribute(string staticName, string name, AttributeTypeEnum type, string notes, bool? visibleInEditUi, object defaultValue)
        {
            StaticName = staticName;
            Type = type.ToString();
            AttributeMetaData = new List<ImpEntity> { GetAttributeMetaData(name, notes, visibleInEditUi, HelpersToRefactor.SerializeValue(defaultValue)) };
        }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static ImpAttribute StringAttribute(string staticName, string name, string notes, bool? visibleInEditUi, string inputType = null, int? rowCount = null, string defaultValue = null)
        {
            var attribute = new ImpAttribute(staticName, name, AttributeTypeEnum.String, notes, visibleInEditUi, defaultValue);
            attribute.AttributeMetaData.Add(attribute.GetStringAttributeMetaData(inputType, rowCount));
            return attribute;
        }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static ImpAttribute BooleanAttribute(string staticName, string name, string notes, bool? visibleInEditUi, bool? defaultValue = null)
        {
            var attribute = new ImpAttribute(staticName, name, AttributeTypeEnum.Boolean, notes, visibleInEditUi, defaultValue);
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
                Values = new Dictionary<string, List<IValueImportModel>>()
            };
            if (!string.IsNullOrEmpty(name))
                allEntity.Values.Add("Name", new List<IValueImportModel> { new ValueImportModel<string>(allEntity) { Value = name } });
            if (!string.IsNullOrEmpty(notes))
                allEntity.Values.Add("Notes", new List<IValueImportModel> { new ValueImportModel<string>(allEntity) { Value = notes } });
            if (visibleInEditUi.HasValue)
                allEntity.Values.Add("VisibleInEditUI", new List<IValueImportModel> { new ValueImportModel<bool?>(allEntity) { Value = visibleInEditUi } });
            if (defaultValue != null)
                allEntity.Values.Add("DefaultValue", new List<IValueImportModel> { new ValueImportModel<string>(allEntity) { Value = defaultValue } });

            return allEntity;
        }

        private ImpEntity GetStringAttributeMetaData(string inputType, int? rowCount)
        {
            var stringEntity = new ImpEntity
            {
                AttributeSetStaticName = "@String",
                Values = new Dictionary<string, List<IValueImportModel>>()
            };
            if (!string.IsNullOrEmpty(inputType))
                stringEntity.Values.Add("InputType", new List<IValueImportModel> { new ValueImportModel<string>(stringEntity) { Value = inputType } });
            if (rowCount.HasValue)
                stringEntity.Values.Add("RowCount", new List<IValueImportModel> { new ValueImportModel<decimal?>(stringEntity) { Value = rowCount } });

            return stringEntity;
        }
    }
}
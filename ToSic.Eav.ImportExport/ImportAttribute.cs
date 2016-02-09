using System.Collections.Generic;

namespace ToSic.Eav.Import
{
    public class ImportAttribute
    {
        public string StaticName { get; set; }
        public string Type { get; set; }

        public string InputType { get; set; }
        public List<ImportEntity> AttributeMetaData { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ImportAttribute() { }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        private ImportAttribute(string staticName, string name, AttributeTypeEnum type, string notes, bool? visibleInEditUi, object defaultValue)
        {
            StaticName = staticName;
            Type = type.ToString();
            AttributeMetaData = new List<ImportEntity> { GetAttributeMetaData(name, notes, visibleInEditUi, HelpersToRefactor.SerializeValue(defaultValue)) };
        }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static ImportAttribute StringAttribute(string staticName, string name, string notes, bool? visibleInEditUi, string inputType = null, int? rowCount = null, string defaultValue = null)
        {
            var attribute = new ImportAttribute(staticName, name, AttributeTypeEnum.String, notes, visibleInEditUi, defaultValue);
            attribute.AttributeMetaData.Add(GetStringAttributeMetaData(inputType, rowCount));
            return attribute;
        }

        /// <summary>
        /// Get an Import-Attribute
        /// </summary>
        public static ImportAttribute BooleanAttribute(string staticName, string name, string notes, bool? visibleInEditUi, bool? defaultValue = null)
        {
            var attribute = new ImportAttribute(staticName, name, AttributeTypeEnum.Boolean, notes, visibleInEditUi, defaultValue);
            return attribute;
        }

        /// <summary>
        /// Shortcut to get an @All Entity Describing an Attribute
        /// </summary>
        private static ImportEntity GetAttributeMetaData(string name, string notes, bool? visibleInEditUi, string defaultValue = null)
        {
            var allEntity = new ImportEntity
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

        private static ImportEntity GetStringAttributeMetaData(string inputType, int? rowCount)
        {
            var stringEntity = new ImportEntity
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
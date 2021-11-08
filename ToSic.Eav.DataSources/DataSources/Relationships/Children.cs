using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Will get the ??? children of an item based on the field they are in.
    ///
    /// Options - unclear?
    ///
    /// Scenarios
    /// 1. Just get all the children of the items involved - all fields
    /// 1. Just get all the children of the items involved - specific field
    /// 1. Allow multiple streams? 
    /// 1. Count? the thildren of items
    /// </summary>

    public class Children : DataSourceBase
    {
        /// <inheritdoc/>
        [PrivateApi]
        public override string LogId => $"{DataSourceConstants.LogPrefix}.RelLok";

        [PrivateApi] public const string FieldNameKey = "FieldName";

        [PrivateApi] public const string ContentTypeNameKey = "ContentTypeName";

        /// <summary>
        /// Name of the field pointing to the children.
        /// If left blank, will use get all children.
        /// </summary>
        public string FieldName
        {
            get => Configuration[FieldNameKey];
            set => Configuration[FieldNameKey] = value;
        }

        /// <summary>
        /// Name of the content-type to get. 
        /// If specified, would only keep the children of this content-type.
        ///
        /// Can usually be left empty (recommended).
        /// </summary>
        public string ContentTypeName
        {
            get => Configuration[ContentTypeNameKey];
            set => Configuration[ContentTypeNameKey] = value;
        }


        /// <summary>
        /// Constructor - DI Compatible - don't call yourself
        /// </summary>
        public Children()
        {
            Provide(GetChildren);
            ConfigMask(FieldNameKey, $"[Settings:{FieldNameKey}]");
            ConfigMask(ContentTypeNameKey, $"[Settings:{ContentTypeNameKey}]");
        }

        private IImmutableList<IEntity> GetChildren()
        {
            var wrapLog = Log.Call<IImmutableList<IEntity>>();

            // Make sure we have an In - otherwise error
            if (!GetRequiredInList(out var originals))
                return wrapLog("error", originals);

            var fieldName = FieldName;
            if (string.IsNullOrWhiteSpace(fieldName)) fieldName = null;
            Log.Add($"Field Name: {fieldName}");
            
            var typeName = ContentTypeName;
            if (string.IsNullOrWhiteSpace(typeName)) typeName = null;
            Log.Add($"Content Type Name: {typeName}");

            var result = originals
                .SelectMany(o => o.Relationships.FindChildren(fieldName, typeName, Log))
                .ToImmutableList();

            return wrapLog(null, result);
        }
    }
}

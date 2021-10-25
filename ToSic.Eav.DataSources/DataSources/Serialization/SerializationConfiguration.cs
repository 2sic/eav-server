using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Eav.Serialization;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// DataSource which changes how Streams will be serialized in the end.
    /// </summary>
    /// <remarks>
    /// New in v11.20
    /// </remarks>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Serialization Configuration",
        UiHint = "Determine how this data is Serialized",
        Icon = "settings_ethernet",
        Type = DataSourceType.Modify, 
        GlobalName = "2952e680-4aaa-4a12-adf7-325cb2854358",
        DynamicOut = true,
        In = new []{Constants.DefaultStreamName},
	    ExpectsDataOfType = "5c84cd3f-f853-40b3-81cf-dee6a07dc411",
        HelpLink = "https://r.2sxc.org/DsSerializationConfiguration")]

    public partial class SerializationConfiguration : DataSourceBase
	{
        #region Constants

        public static string KeepAll = "*";
        public static string KeepNone = "-";

        #endregion
        #region Configuration-properties
        private const string IncludeIdKey = "IncludeId";
        private const string IncludeGuidKey = "IncludeGuid";
        private const string IncludeTitleKey = "IncludeTitle";
        private const string IncludeCreatedKey = "IncludeCreated";
        private const string IncludeModifiedKey = "IncludeModified";

        private const string RmvNullValuesKey = "RemoveNullValues";
        private const string RmvZeroValuesKey = "RemoveZeroValues";
        private const string RmvEmptyStringsKey = "RemoveEmptyStringValues";
        private const string RmvBooleanFalseKey = "RemoveFalseValues";

        private const string IncludeMetadataForKey = "IncludeMetadataFor";
        private const string IncludeMetadataForIdKey = "IncludeMetadataForId";
        private const string IncludeMetadataForTypeKey = "IncludeMetadataForType";

        private const string IncludeMetadataKey = "IncludeMetadata";
        private const string IncludeMetadataIdKey = "IncludeMetadataId";
        private const string IncludeMetadataGuidKey = "IncludeMetadataGuid";
        private const string IncludeMetadataTitleKey = "IncludeMetadataTitle";

        private const string IncludeRelationshipsKey = "IncludeRelationships";
        private const string IncludeRelationshipIdKey = "IncludeRelationshipId";
        private const string IncludeRelationshipGuidKey = "IncludeRelationshipGuid";
        private const string IncludeRelationshipTitleKey = "IncludeRelationshipTitle";

        /// <inheritdoc/>
        [PrivateApi]
        public override string LogId => "DS.SerCnf";

        #region Basic Fields

        /// <summary>
        /// Should the ID be included in serialization
        /// </summary>
        public string IncludeId { get => Configuration[IncludeIdKey]; set => Configuration[IncludeIdKey] = value; }
        
        /// <summary>
        /// Should the GUID be included in serialization
        /// </summary>
        public string IncludeGuid { get => Configuration[IncludeGuidKey]; set => Configuration[IncludeGuidKey] = value; }
        
        /// <summary>
        /// Should the default Title be included as "Title" in serialization
        /// </summary>
        public string IncludeTitle { get => Configuration[IncludeTitleKey]; set => Configuration[IncludeTitleKey] = value; }

        #endregion

        #region Dates

        /// <summary>
        /// Should the Modified date be included in serialization
        /// </summary>
        public string IncludeModified { get => Configuration[IncludeModifiedKey]; set => Configuration[IncludeModifiedKey] = value; }
        
        /// <summary>
        /// Should the Created date be included in serialization
        /// </summary>
        public string IncludeCreated { get => Configuration[IncludeCreatedKey]; set => Configuration[IncludeCreatedKey] = value; }

        #endregion

        #region Optimize - new in 12.05

        /// <summary>
        /// todo
        /// </summary>
        public string RemoveNullValues { get => Configuration[RmvNullValuesKey]; set => Configuration[RmvNullValuesKey] = value; }

        /// <summary>
        /// todo
        /// </summary>
        public string RemoveZeroValues { get => Configuration[RmvZeroValuesKey]; set => Configuration[RmvZeroValuesKey] = value; }

        /// <summary>
        /// todo
        /// </summary>
        public string RemoveEmptyStrings { get => Configuration[RmvEmptyStringsKey]; set => Configuration[RmvEmptyStringsKey] = value; }

        /// <summary>
        /// todo
        /// </summary>
        public string DropFalseValues { get => Configuration[RmvBooleanFalseKey]; set => Configuration[RmvBooleanFalseKey] = value; }

        #endregion

        #region Metadata For - enhanced in 12.05

        /// <summary>
        /// Should the Metadata target/for information be included in serialization
        /// </summary>
        public string IncludeMetadataFor { get => Configuration[IncludeMetadataForKey]; set => Configuration[IncludeMetadataForKey] = value; }

        /// <summary>
        /// Should the Metadata target/for information be included in serialization
        /// </summary>
        public string IncludeMetadataForId { get => Configuration[IncludeMetadataForIdKey]; set => Configuration[IncludeMetadataForIdKey] = value; }

        /// <summary>
        /// Should the Metadata target/for information be included in serialization
        /// </summary>
        public string IncludeMetadataForType { get => Configuration[IncludeMetadataForTypeKey]; set => Configuration[IncludeMetadataForTypeKey] = value; }
        #endregion

        #region Metadata

        /// <summary>
        /// Should the Metadata ID be included in serialization
        /// </summary>
        public string IncludeMetadata { get => Configuration[IncludeMetadataKey]; set => Configuration[IncludeMetadataKey] = value; }

        /// <summary>
        /// Should the Metadata ID be included in serialization
        /// </summary>
        public string IncludeMetadataId { get => Configuration[IncludeMetadataIdKey]; set => Configuration[IncludeMetadataIdKey] = value; }

        /// <summary>
        /// Should the Metadata GUID be included in serialization
        /// </summary>
        public string IncludeMetadataGuid { get => Configuration[IncludeMetadataGuidKey]; set => Configuration[IncludeMetadataGuidKey] = value; }

        /// <summary>
        /// Should the default Title of the Metadata be included as "Title" in serialization
        /// </summary>
        public string IncludeMetadataTitle { get => Configuration[IncludeMetadataTitleKey]; set => Configuration[IncludeMetadataTitleKey] = value; }

        #endregion

        #region Relationships


        /// <summary>
        /// Should the Relationship ID be included in serialization
        /// </summary>
        public string IncludeRelationships { get => Configuration[IncludeRelationshipsKey]; set => Configuration[IncludeRelationshipsKey] = value; }

        /// <summary>
        /// Should the Relationship ID be included in serialization
        /// </summary>
        public string IncludeRelationshipId { get => Configuration[IncludeRelationshipIdKey]; set => Configuration[IncludeRelationshipIdKey] = value; }

        /// <summary>
        /// Should the Relationship GUID be included in serialization
        /// </summary>
        public string IncludeRelationshipGuid { get => Configuration[IncludeRelationshipGuidKey]; set => Configuration[IncludeRelationshipGuidKey] = value; }

        /// <summary>
        /// Should the default Title of the Relationship be included as "Title" in serialization
        /// </summary>
        public string IncludeRelationshipTitle { get => Configuration[IncludeRelationshipTitleKey]; set => Configuration[IncludeRelationshipTitleKey] = value; }
        #endregion


        #endregion


        /// <inheritdoc />
        /// <summary>
        /// Constructs a new AttributeFilter DataSource
        /// </summary>
        [PrivateApi]
		public SerializationConfiguration()
		{
            // Basic system properties
            ConfigMask(IncludeIdKey, $"[Settings:{IncludeIdKey}]");
            ConfigMask(IncludeGuidKey, $"[Settings:{IncludeGuidKey}]");
            ConfigMask(IncludeTitleKey, $"[Settings:{IncludeTitleKey}]");
            
            // Dates
            ConfigMask(IncludeCreatedKey, $"[Settings:{IncludeCreatedKey}]");
            ConfigMask(IncludeModifiedKey, $"[Settings:{IncludeModifiedKey}]");
            
            // Optimize output - enhanced in 12.05
            ConfigMask(RmvNullValuesKey, $"[Settings:{RmvNullValuesKey}]");
            ConfigMask(RmvZeroValuesKey, $"[Settings:{RmvZeroValuesKey}]");
            ConfigMask(RmvEmptyStringsKey, $"[Settings:{RmvEmptyStringsKey}]");
            ConfigMask(RmvBooleanFalseKey, $"[Settings:{RmvBooleanFalseKey}]");

            // Metadata For - enhanced in 12.05
            ConfigMask(IncludeMetadataForKey, $"[Settings:{IncludeMetadataForKey}]");
            ConfigMask(IncludeMetadataForIdKey, $"[Settings:{IncludeMetadataForTypeKey}]");
            ConfigMask(IncludeMetadataForTypeKey, $"[Settings:{IncludeMetadataForTypeKey}]");

            // Metadata
            ConfigMask(IncludeMetadataKey, $"[Settings:{IncludeMetadataKey}]");
            ConfigMask(IncludeMetadataIdKey, $"[Settings:{IncludeMetadataIdKey}]");
            ConfigMask(IncludeMetadataGuidKey, $"[Settings:{IncludeMetadataGuidKey}]");
            ConfigMask(IncludeMetadataTitleKey, $"[Settings:{IncludeMetadataTitleKey}]");

            // Relationships
            ConfigMask(IncludeRelationshipsKey, $"[Settings:{IncludeRelationshipsKey}]");
            ConfigMask(IncludeRelationshipIdKey, $"[Settings:{IncludeRelationshipIdKey}]");
            ConfigMask(IncludeRelationshipGuidKey, $"[Settings:{IncludeRelationshipGuidKey}]");
            ConfigMask(IncludeRelationshipTitleKey, $"[Settings:{IncludeRelationshipTitleKey}]");
        }

        /// <summary>
        /// Get the list of all items with reduced attributes-list
        /// </summary>
        /// <returns></returns>
		private IImmutableList<IEntity> GetList(string inStreamName = Constants.DefaultStreamName)
        {
            var wrapLog = Log.Call<IImmutableList<IEntity>>();
            Configuration.Parse();

            var original = In[inStreamName].List.ToImmutableList();

            var enhanced = AddSerializationRules(original);
            
		    return wrapLog($"{enhanced.Count}", enhanced);
		}

        private IImmutableList<IEntity> AddSerializationRules(IImmutableList<IEntity> before)
        {
            var wrapLog = Log.Call<IImmutableList<IEntity>>();
            // Skip if no rules defined
            var noRules = string.IsNullOrWhiteSpace(string.Join("", Configuration));
            if(noRules) return wrapLog("no rules, unmodified", before);

            var id = TryParseIncludeRule(IncludeId);
            var title = TryParseIncludeRule(IncludeTitle);
            var guid = TryParseIncludeRule(IncludeGuid);
            var created = TryParseIncludeRule(IncludeCreated);
            var modified = TryParseIncludeRule(IncludeModified);
            
            var dropNullValues = TryParseIncludeRule(RemoveNullValues) ?? false;
            var dropZeroValues = TryParseIncludeRule(RemoveZeroValues) ?? false;
            var dropEmptyStringValues = TryParseIncludeRule(RemoveEmptyStrings) ?? false;
            var dropFalseValues = TryParseIncludeRule(DropFalseValues) ?? false;

            var mdForSer = new MetadataForSerialization
            {
                Serialize = TryParseIncludeRule(IncludeMetadataFor),
                SerializeKey = TryParseIncludeRule(IncludeMetadataForId),
                SerializeType = TryParseIncludeRule(IncludeMetadataForType),
            };

            var mdSer = new SubEntitySerialization
            {
                Serialize = TryParseIncludeRule(IncludeMetadata),
                SerializeId = TryParseIncludeRule(IncludeMetadataId),
                SerializeGuid = TryParseIncludeRule(IncludeMetadataGuid),
                SerializeTitle = TryParseIncludeRule(IncludeMetadataTitle)
            };


            var relSer = new SubEntitySerialization
            {
                Serialize = TryParseIncludeRule(IncludeRelationships),
                SerializeId = TryParseIncludeRule(IncludeRelationshipId),
                SerializeGuid = TryParseIncludeRule(IncludeRelationshipGuid),
                SerializeTitle = TryParseIncludeRule(IncludeRelationshipTitle)
            };

            var decorator = new EntitySerializationDecorator
            {
                RemoveNullValues = dropNullValues,
                RemoveZeroValues = dropZeroValues,
                RemoveEmptyStringValues = dropEmptyStringValues,
                RemoveBoolFalseValues = dropFalseValues,

                // Metadata & Relationships
                SerializeMetadataFor = mdForSer,
                SerializeMetadata = mdSer,
                SerializeRelationships = relSer,

                // id, title, guid
                SerializeId = id,
                SerializeTitle = title,
                SerializeGuid = guid,

                // dates
                SerializeCreated = created,
                SerializeModified = modified
            };

            var result = before
                .Select(e => (IEntity)new EntityDecorator12<EntitySerializationDecorator>(e, decorator));

            return wrapLog("modified", result.ToImmutableList());
        }
        
        private bool? TryParseIncludeRule(string original)
            => bool.TryParse(original, out var include) ? (bool?)include : null;

    }
}
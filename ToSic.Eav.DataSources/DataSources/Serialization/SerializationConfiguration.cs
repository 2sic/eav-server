using System.Collections.Immutable;
using System.Linq;
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
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Serialization Configuration",
        UiHint = "Determine how this data is Serialized",
        Icon = "settings_ethernet",
        Type = DataSourceType.Modify, 
        GlobalName = "2952e680-4aaa-4a12-adf7-325cb2854358",
        DynamicOut = false,
        In = new []{Constants.DefaultStreamName},
	    ExpectsDataOfType = "5c84cd3f-f853-40b3-81cf-dee6a07dc411",
        HelpLink = "https://r.2sxc.org/DsSerializationConfiguration")]

    public class SerializationConfiguration : DataSourceBase
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
        
        private const string IncludeMetadataForKey = "IncludeMetadataFor";
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

        #region Metadata
        /// <summary>
        /// Should the Metadata target/for information be included in serialization
        /// </summary>
        public string IncludeMetadataFor { get => Configuration[IncludeMetadataForKey]; set => Configuration[IncludeMetadataForKey] = value; }

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
            Provide(GetList);
            
            // Basic system properties
            ConfigMask(IncludeIdKey, $"[Settings:{IncludeIdKey}]");
            ConfigMask(IncludeGuidKey, $"[Settings:{IncludeGuidKey}]");
            ConfigMask(IncludeTitleKey, $"[Settings:{IncludeTitleKey}]");
            
            // Dates
            ConfigMask(IncludeCreatedKey, $"[Settings:{IncludeCreatedKey}]");
            ConfigMask(IncludeModifiedKey, $"[Settings:{IncludeModifiedKey}]");
            
            // Metadata
            ConfigMask(IncludeMetadataForKey, $"[Settings:{IncludeMetadataForKey}]");
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
		private IImmutableList<IEntity> GetList()
        {
            var wrapLog = Log.Call<IImmutableList<IEntity>>();
            Configuration.Parse();


            var result = In[Constants.DefaultStreamName].Immutable;

            result = AddSerializationRules(result);
            
		    return wrapLog($"{result.Count}", result);
		}

        private IImmutableList<IEntity> AddSerializationRules(IImmutableList<IEntity> before)
        {
            var wrapLog = Log.Call<IImmutableList<IEntity>>();
            // Skip if no rules defined
            var noRules = string.IsNullOrWhiteSpace(""
                + IncludeId + IncludeGuid + IncludeTitle 
                + IncludeCreated + IncludeModified
                + IncludeMetadataFor
                + IncludeMetadata + IncludeMetadataId + IncludeMetadataGuid + IncludeMetadataTitle
                + IncludeRelationships + IncludeRelationshipId + IncludeRelationshipGuid + IncludeRelationshipTitle);
            if(noRules) return wrapLog("no rules, unmodified", before);

            var id = TryParseIncludeRule(IncludeId);
            var title = TryParseIncludeRule(IncludeTitle);
            var guid = TryParseIncludeRule(IncludeGuid);
            var created = TryParseIncludeRule(IncludeCreated);
            var modified = TryParseIncludeRule(IncludeModified);
            
            var mdFor = TryParseIncludeRule(IncludeMetadataFor);
            
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


            var result = before.Select(selector: e =>
            {
                var newEntity = new EntityWithSerialization(e);
                if (id != null) newEntity.SerializeId = id;
                if (title != null) newEntity.SerializeTitle = title;
                if (guid != null) newEntity.SerializeGuid = guid;
                
                // dates
                if (created != null) newEntity.SerializeCreated = created;
                if (modified != null) newEntity.SerializeModified = modified;
                
                // Metadata & Relationships
                if (mdFor != null) newEntity.SerializeMetadataFor = mdFor;
                newEntity.SerializeMetadata = mdSer;
                newEntity.SerializeRelationships = relSer;

                return (IEntity) newEntity;
            });

            return wrapLog("modified", result.ToImmutableList());
        }
        
        private bool? TryParseIncludeRule(string original)
            => bool.TryParse(original, out var include) ? (bool?)include : null;
    }
}
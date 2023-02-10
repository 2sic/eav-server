using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Logging;
using ToSic.Eav.Serialization;
using ToSic.Lib.Documentation;
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
        Icon = Icons.HtmlDotDotDot,
        Type = DataSourceType.Modify, 
        GlobalName = "2952e680-4aaa-4a12-adf7-325cb2854358",
        DynamicOut = true,
        In = new []{Constants.DefaultStreamName},
	    ExpectsDataOfType = "5c84cd3f-f853-40b3-81cf-dee6a07dc411",
        HelpLink = "https://r.2sxc.org/DsSerializationConfiguration")]

    public partial class SerializationConfiguration : DataSource
	{
        #region Constants

        public static string KeepAll = "*";
        public static string KeepNone = "-";

        #endregion
        #region Configuration-properties
        private const string RmvEmptyStringsKey = "RemoveEmptyStringValues";
        private const string RmvBooleanFalseKey = "RemoveFalseValues";

        #region Basic Fields

        /// <summary>
        /// Should the ID be included in serialization
        /// </summary>
        public string IncludeId { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// Should the AppId be included in serialization.
        /// Especially for scenarios where data is retrieved from multiple Apps
        /// </summary>
        public string IncludeAppId { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// Should the AppId be included in serialization.
        /// Especially for scenarios where data is retrieved from multiple Apps
        /// </summary>
        public string IncludeZoneId { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// Should the GUID be included in serialization
        /// </summary>
        public string IncludeGuid { get => Configuration.GetThis(); set => Configuration.SetThis(value); }
        
        /// <summary>
        /// Should the default Title be included as "Title" in serialization
        /// </summary>
        public string IncludeTitle { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        #endregion

        #region Dates

        /// <summary>
        /// Should the Modified date be included in serialization
        /// </summary>
        public string IncludeModified { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// Should the Created date be included in serialization
        /// </summary>
        public string IncludeCreated { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        #endregion

        #region Optimize - new in 12.05

        /// <summary>
        /// todo
        /// </summary>
        public string RemoveNullValues { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// todo
        /// </summary>
        public string RemoveZeroValues { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// todo
        /// </summary>
        public string RemoveEmptyStrings { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// todo
        /// </summary>
        public string DropFalseValues { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        #endregion

        #region Metadata For - enhanced in 12.05

        /// <summary>
        /// Should the Metadata target/for information be included in serialization
        /// </summary>
        public string IncludeMetadataFor { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// Should the Metadata target/for information be included in serialization
        /// </summary>
        public string IncludeMetadataForId { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// Should the Metadata target/for information be included in serialization
        /// </summary>
        public string IncludeMetadataForType { get => Configuration.GetThis(); set => Configuration.SetThis(value); }
        #endregion

        #region Metadata

        /// <summary>
        /// Should the Metadata ID be included in serialization
        /// </summary>
        public string IncludeMetadata { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// Should the Metadata ID be included in serialization
        /// </summary>
        public string IncludeMetadataId { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// Should the Metadata GUID be included in serialization
        /// </summary>
        public string IncludeMetadataGuid { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// Should the default Title of the Metadata be included as "Title" in serialization
        /// </summary>
        public string IncludeMetadataTitle { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        #endregion

        #region Relationships


        /// <summary>
        /// Should the Relationship ID be included in serialization
        /// </summary>
        public string IncludeRelationships { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// Should the Relationship ID be included in serialization
        /// </summary>
        public string IncludeRelationshipId { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// Should the Relationship GUID be included in serialization
        /// </summary>
        public string IncludeRelationshipGuid { get => Configuration.GetThis(); set => Configuration.SetThis(value); }

        /// <summary>
        /// Should the default Title of the Relationship be included as "Title" in serialization
        /// </summary>
        public string IncludeRelationshipTitle { get => Configuration.GetThis(); set => Configuration.SetThis(value); }
        #endregion


        #endregion


        /// <inheritdoc />
        /// <summary>
        /// Constructs a new AttributeFilter DataSource
        /// </summary>
        [PrivateApi]
		public SerializationConfiguration(Dependencies dependencies) : base(dependencies, $"{DataSourceConstants.LogPrefix}.SerCnf")
        {
            // Basic system properties
            ConfigMask(nameof(IncludeId));
            ConfigMask(nameof(IncludeGuid));
            ConfigMask(nameof(IncludeTitle));
            ConfigMask(nameof(IncludeAppId));
            ConfigMask(nameof(IncludeZoneId));

            // Dates
            ConfigMask(nameof(IncludeCreated));
            ConfigMask(nameof(IncludeModified));
            
            // Optimize output - enhanced in 12.05
            ConfigMask(nameof(RemoveNullValues));
            ConfigMask(nameof(RemoveZeroValues));
            ConfigMaskMyConfig(nameof(RemoveEmptyStrings), RmvEmptyStringsKey);
            ConfigMaskMyConfig(nameof(DropFalseValues), RmvBooleanFalseKey);

            // Metadata For - enhanced in 12.05
            ConfigMask(nameof(IncludeMetadataFor));
            ConfigMask(nameof(IncludeMetadataForId));
            ConfigMask(nameof(IncludeMetadataForType));

            // Metadata
            ConfigMask(nameof(IncludeMetadata));
            ConfigMask(nameof(IncludeMetadataId));
            ConfigMask(nameof(IncludeMetadataGuid));
            ConfigMask(nameof(IncludeMetadataTitle));

            // Relationships
            ConfigMask(nameof(IncludeRelationships));
            ConfigMask(nameof(IncludeRelationshipId));
            ConfigMask(nameof(IncludeRelationshipGuid));
            ConfigMask(nameof(IncludeRelationshipTitle));
        }

        /// <summary>
        /// Get the list of all items with reduced attributes-list
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> GetList(string inStreamName = Constants.DefaultStreamName) => Log.Func(() =>
        {
            Configuration.Parse();
            var original = In[inStreamName].List.ToImmutableList();
            var enhanced = AddSerializationRules(original);
            return (enhanced, $"{enhanced.Count}");
        });

        private IImmutableList<IEntity> AddSerializationRules(IImmutableList<IEntity> before) => Log.Func(() =>
        {
            // Skip if no rules defined
            var noRules = string.IsNullOrWhiteSpace(string.Join("", Configuration));
            if (noRules) return (before, "no rules, unmodified");

            var id = TryParseIncludeRule(IncludeId);
            var title = TryParseIncludeRule(IncludeTitle);
            var guid = TryParseIncludeRule(IncludeGuid);
            var created = TryParseIncludeRule(IncludeCreated);
            var modified = TryParseIncludeRule(IncludeModified);
            var appId = TryParseIncludeRule(IncludeAppId);
            var zoneId = TryParseIncludeRule(IncludeZoneId);

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
                SerializeAppId = appId,
                SerializeZoneId = zoneId,

                // dates
                SerializeCreated = created,
                SerializeModified = modified
            };

            var result = before
                .Select(e => (IEntity)new EntityDecorator12<EntitySerializationDecorator>(e, decorator));

            return (result.ToImmutableList(), "modified");
        });
        
        private bool? TryParseIncludeRule(string original)
            => bool.TryParse(original, out var include) ? (bool?)include : null;

    }
}
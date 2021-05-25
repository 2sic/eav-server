using System;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;

namespace ToSic.Eav
{
    public partial class Constants
    {
        /// <summary>
        /// Name of the Default App in all Zones
        /// </summary>
        public const string DefaultAppName = "Default";

        public const int AppIdEmpty = 0;

        public const int NullId = -2742;
        public const int IdNotInitialized = -999;
        public const string UrlNotInitialized = "url-not-initialized";

        public const string CultureSystemKey = "Culture";

        /// <summary>
        /// DataTimeline Operation-Key for Entity-States (Entity-Versioning)
        /// </summary>
        public const string DataTimelineEntityJson = "e";



        /// <summary>
        /// Default ZoneId. Used if none is specified on the Context.
        /// </summary>
        public static readonly int DefaultZoneId = 1;

        /// <summary>
        /// AppId where MetaData (Entities) are stored.
        /// </summary>
        public static readonly int MetaDataAppId = 1;


        #region Metadata Targets (Obsolete/Moved)
        
        /// <summary>Things that are not used as Metadata</summary>
        [Obsolete("Will be removed in 2sxc 13 - use TargetTypes.None")] public static int NotMetadata = (int)TargetTypes.None; // 1;

        /// <summary>Metadata of attributes (fields)</summary>
        public static readonly int MetadataForAttribute = (int)TargetTypes.Attribute; // 2;

        /// <summary>App metadata</summary>
        public static readonly int MetadataForApp = (int)TargetTypes.App;//  3;

        /// <summary>Metadata of entities (data-items)</summary>
        public const int MetadataForEntity = (int)TargetTypes.Entity; // 4;

        /// <summary>Metadata of a content-type (data-schema)</summary>
        public static readonly int MetadataForContentType = (int)TargetTypes.MetadataForContentType; // 5;

        /// <summary>Zone metadata</summary>
        // ReSharper disable once UnusedMember.Global
        [Obsolete("Will be removed in 2sxc 13 - use TargetTypes.Zone")] public static readonly int MetadataForZone = (int)TargetTypes.Zone; // 6;

        public static readonly int MetadataForCmsObject = (int)TargetTypes.CmsItem; // 10;
        #endregion

        #region Content-Types Constants (Obsolete/Moved)

        [Obsolete("Will be removed in 2sxc 13 - use ContentTypes")] public const string ContentTypeTypeName = ContentTypes.ContentTypeTypeName;
        [Obsolete("Will be removed in 2sxc 13 - use ContentTypes")] public static readonly string ContentTypeMetadataLabel = ContentTypes.ContentTypeMetadataLabel;

        #endregion

        #region DataSource Constants

        /// <summary>content type name of the query AttributeSet</summary>
        public static readonly string QueryTypeName = "DataPipeline";

        /// <summary>content-type name of the queryPart AttributeSet</summary>
        public static readonly string QueryPartTypeName = "DataPipelinePart";

        /// <summary>
        /// Attribute Name on the query-Entity describing the Stream-Wiring
        /// </summary>
        public const string QueryStreamWiringAttributeName = "StreamWiring";

        /// <summary>
        /// Default In-/Out-Stream Name
        /// </summary>
        public const string DefaultStreamName = "Default";
        public const string DefaultStreamNameRequired = DefaultStreamName + "*";

        public const string FallbackStreamName = "Fallback";

        /// <summary>PublishedEntities Stream Name</summary>
        public const string PublishedStreamName = "Published";

        /// <summary>Draft-Entities Stream Name</summary>
        public const string DraftsStreamName = "Drafts";


        #endregion



        #region Scopes

        public const string ScopeSystem = "System";

        #endregion


        #region Special Attributes / Fields of Entities in lower-case (Obsolete / Moved)

        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldTitle = Attributes.EntityFieldTitle;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldId = Attributes.EntityFieldId;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldAutoSelect = Attributes.EntityFieldAutoSelect;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldGuid = Attributes.EntityFieldGuid;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldType = Attributes.EntityFieldType;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldIsPublished = Attributes.EntityFieldIsPublished;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldCreated = Attributes.EntityFieldCreated;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldModified = Attributes.EntityFieldModified;

        #endregion


        #region Special Field Types (Obsolete / Moved)

        [Obsolete("Will be removed in 2sxc 13 - use the new DataTypes")] public const string DataTypeBoolean = DataTypes.Boolean;
        [Obsolete("Will be removed in 2sxc 13 - use the new DataTypes")] public const string DataTypeNumber = DataTypes.Number;
        [Obsolete("Will be removed in 2sxc 13 - use the new DataTypes")] public const string DataTypeDateTime = DataTypes.DateTime;
        [Obsolete("Will be removed in 2sxc 13 - use the new DataTypes")] public const string DataTypeEntity = DataTypes.Entity;
        [Obsolete("Will be removed in 2sxc 13 - use the new DataTypes")] public const string DataTypeHyperlink = DataTypes.Hyperlink;
        [Obsolete("Will be removed in 2sxc 13 - use the new DataTypes")] public const string DataTypeString = DataTypes.String;

        #endregion


        #region special uses of Apps
        
        public const string ContentAppName = "Content";
        public const string ContentAppFolder = "Content";
        public const string AppAssignmentName = "App";

        #endregion

        public const string DynamicType = "dynamic";

        public const int TransientAppId = -9999999;
        public const int SystemContentTypeFakeParent = -9203503; // just a very strange, dummy number

        #region Parameter protection (Obsolete / Moved)

        // Special constant to protect functions which should use named parameters
        [Obsolete("Will be removed in 2sxc 13 - use the new Parameters")] 
        public const string RandomProtectionParameter = Parameters.Protector;

        // ReSharper disable once UnusedParameter.Local

        [Obsolete("Will be removed in 2sxc 13 - use the new Parameters")]
        public static void ProtectAgainstMissingParameterNames(string criticalParameter, string protectedMethod, string paramNames) 
            => Parameters.ProtectAgainstMissingParameterNames(criticalParameter, protectedMethod, paramNames);

        #endregion
    }
}

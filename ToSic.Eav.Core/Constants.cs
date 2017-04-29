using System;
using System.Text.RegularExpressions;

namespace ToSic.Eav
{
    public partial class Constants
    {
        /// <summary>
        /// Name of the Default App in all Zones
        /// </summary>
        public const string DefaultAppName = "Default";


        public const string CultureSystemKey = "Culture";
        /// <summary>
        /// DataTimeline Operation-Key for Entity-States (Entity-Versioning)
        /// </summary>
        public const string DataTimelineEntityStateOperation = "s";

        #region DB Field / Names Constants

        /// <summary>
        /// AttributeSet StaticName must match this Regex. Accept Alphanumeric, except the first char must be alphabetic or underscore.
        /// </summary>
        //public static string AttributeStaticNameRegEx = "^[_a-zA-Z]{1}[_a-zA-Z0-9]*"; // todo now: create static regex...
        public static Regex AttributeStaticName = new Regex("^[_a-zA-Z]{1}[_a-zA-Z0-9]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// If AttributeSet StaticName doesn't match, users see this message.
        /// </summary>
        public static string AttributeStaticNameRegExNotes = "Only alphanumerics and underscore is allowed, first char must be alphabetic or underscore.";

        #endregion

        #region DataSource Constants

        /// <summary>
        /// Default ZoneId. Used if none is specified on the Context.
        /// </summary>
        public static readonly int DefaultZoneId = 1;
        /// <summary>
        /// AppId where MetaData (Entities) are stored.
        /// </summary>
        public static readonly int MetaDataAppId = 1;


        /// <summary>
        /// Default Entity AssignmentObjectTypeId
        /// </summary>
        [Obsolete("Use NotMetadata instead")]
        public const int AssignmentObjectTypeId = 1;
        public const int NotMetadata = 1;

        /// <summary>
        /// AssignmentObjectTypeId for FieldProperties (Field MetaData)
        /// </summary>
        [Obsolete("Use MetadataForField instead")]
        public static readonly int AssignmentObjectTypeIdFieldProperties = 2;
        public static readonly int MetadataForField = 2;

        /// <summary>
        /// AssignmentObjectTypeId for DataPipelines
        /// </summary>
        [Obsolete("Use MetadataForEntity instead")]
        public static readonly int AssignmentObjectTypeEntity = 4;
        public static readonly int MetadataForEntity = 4;

        public static readonly int MetadataForContentType = 5;

        [Obsolete("Use MetadataForCmsObject instead")]
        public static readonly int AssignmentObjectTypeCmsObject = 10; 
        public static readonly int MetadataForCmsObject = 10;

        #region Metadata-Properties which have system use
        public static readonly string ContentTypeMetadataLabel = "Label";
        #endregion

        /// <summary>
        /// StaticName of the DataPipeline AttributeSet
        /// </summary>
        public static readonly string DataPipelineStaticName = "DataPipeline";
        /// <summary>
        /// StaticName of the DataPipelinePart AttributeSet
        /// </summary>
        public static readonly string DataPipelinePartStaticName = "DataPipelinePart";

        /// <summary>
        /// Attribute Name on the Pipeline-Entity describing the Stream-Wiring
        /// </summary>
        public const string DataPipelineStreamWiringStaticName = "StreamWiring";

        /// <summary>
        /// Default In-/Out-Stream Name
        /// </summary>
        public const string DefaultStreamName = "Default";

        public const string FallbackStreamName = "Fallback";

        /// <summary>PublishedEntities Stream Name</summary>
        public const string PublishedStreamName = "Published";
        /// <summary>Draft-Entities Stream Name</summary>
        public const string DraftsStreamName = "Drafts";

        public const string PublishedEntityField = "PublishedEntity";
        public const string DraftEntityField = "DraftEntity";

        public const string TypeForInputTypeDefinition = "ContentType-InputType";
        #endregion


        #region Version Change Constants
        public const string V3To4DataSourceDllOld = ", ToSic.Eav";
        public const string V3To4DataSourceDllNew = ", ToSic.Eav.DataSources";
        #endregion

        #region Scopes

        public const string ScopeSystem = "System";

        #endregion

        #region Special Properties of Entities

        public const string EntityFieldTitle = "entitytitle";
        public const string EntityFieldId = "entityid";
        public const string EntityFieldGuid = "entityguid";
        public const string EntityFieldType = "entitytype";
        public const string EntityFieldIsPublished = "ispublished";
        public const string EntityFieldModified = "modified";

        public static bool InternalOnlyIsSpecialEntityProperty(string name)
        {
            switch (name.ToLower())
            {
                case EntityFieldTitle:
                case EntityFieldId:
                case EntityFieldGuid:
                case EntityFieldType:
                case EntityFieldIsPublished:
                case EntityFieldModified:
                    return true;
            }
            return false;

        }

        #endregion

        #region Special Field Types

        public const string Hyperlink = "Hyperlink";

        #endregion

        #region Special use cases of entities
        public const string ContentKey = "Content";
        public const string ContentKeyLower = "content";

        public const string PresentationKey = "Presentation";
        public const string PresentationKeyLower = "presentation";
        #endregion

        #region special uses of Apps
        public const string ContentAppName = "Content";
        public const string AppAssignmentName = "App";
        #endregion
    }
}

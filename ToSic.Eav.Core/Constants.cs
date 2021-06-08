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




        #region special uses of Apps
        
        public const string ContentAppName = "Content";
        public const string ContentAppFolder = "Content";
        public const string AppAssignmentName = "App";

        #endregion

        public const string DynamicType = "dynamic";

        public const int TransientAppId = -9999999;
        public const int SystemContentTypeFakeParent = -9203503; // just a very strange, dummy number

    }
}

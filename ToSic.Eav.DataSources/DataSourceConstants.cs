using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    [PrivateApi]
    public class DataSourceConstants
    {
        public const string LogPrefix = "DS";
        public static readonly string RootDataSource = typeof(IAppRoot).AssemblyQualifiedName;

        #region Version Change Constants

        internal const string V3To4DataSourceDllOld = ", ToSic.Eav";
        internal const string V3To4DataSourceDllNew = ", ToSic.Eav.DataSources";

        #endregion

        /// <summary>
        /// Global queries must start with this prefix
        /// </summary>
        public const string SystemQueryPrefixPreV15 = "Eav.Queries.Global.";

        public const string SystemQueryPrefix = "System.";

        #region Stream names

        /// <summary>
        /// Default In-/Out-Stream Name
        /// </summary>
        public const string DefaultStreamName = "Default";


        public const string DefaultStreamNameRequired = "Default" + "*";
        public const string FallbackStreamName = "Fallback";


        /// <summary>PublishedEntities Stream Name</summary>
        public const string PublishedStreamName = "Published";

        /// <summary>Draft-Entities Stream Name</summary>
        public const string DraftsStreamName = "Drafts";

        #endregion

        #region Query terms - used for DataSources but also the App project which has a Query Manager

        /// <summary>content type name of the query AttributeSet</summary>
        public static readonly string QueryTypeName = "DataPipeline";

        /// <summary>content-type name of the queryPart AttributeSet</summary>
        public static readonly string QueryPartTypeName = "DataPipelinePart";

        /// <summary>
        /// Attribute Name on the query-Entity describing the Stream-Wiring
        /// </summary>
        public const string QueryStreamWiringAttributeName = "StreamWiring";

        #endregion
    }
}

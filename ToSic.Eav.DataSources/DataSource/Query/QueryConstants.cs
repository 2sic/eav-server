using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources.Queries
{
    /// <summary>
    /// Constants used by Queries / VisualQuery
    /// </summary>
    [PublicApi]
    public class QueryConstants
    {
        #region Public constants - should never get renamed

        /// <summary>
        /// Use this in the `In` stream names array of the of the <see cref="VisualQueryAttribute"/>
        /// to mark an in-stream as being required.
        /// </summary>
        public const string InStreamRequiredSuffix = "*";

        /// <summary>
        /// Marker for specifying that the Default `In` stream is required on the <see cref="VisualQueryAttribute"/>.
        /// </summary>
        public const string InStreamDefaultRequired = "Default" + InStreamRequiredSuffix;

        #endregion

        #region Params (public)

        /// <summary>
        /// The source name to get query parameters.
        /// Usually used in tokens like `[Params:MyParamKey]`
        /// </summary>
        public const string ParamsSourceName = "Params";

        /// <summary>
        /// A special property added to the `Params` source to find out if the current user can see drafts.
        /// </summary>
        public const string ParamsShowDraftsKey = "ShowDrafts";

        /// <summary>
        /// Default for `ShowDrafts` is `false`
        /// </summary>
        public const bool ParamsShowDraftsDefault = false;

        #endregion

        internal const string PartAssemblyAndType = "PartAssemblyAndType";

        //public const string Identifier = "Identifier";

        //public const string TypeNameForUi = "TypeNameForUi";

        internal const string VisualDesignerData = "VisualDesignerData";



        //public const string ParamsShowDraftKeyAndToken = "ShowDrafts||false";

        #region Query terms - used for DataSources but also the App project which has a Query Manager

        /// <summary>content type name of the query AttributeSet</summary>
        internal static readonly string QueryTypeName = "DataPipeline";

        /// <summary>content-type name of the queryPart AttributeSet</summary>
        internal static readonly string QueryPartTypeName = "DataPipelinePart";

        #endregion

        /// <summary>
        /// Attribute Name on the query-Entity describing the Stream-Wiring
        /// </summary>
        [PrivateApi] public const string QueryStreamWiringAttributeName = "StreamWiring";

    }
}
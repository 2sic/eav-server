namespace ToSic.Eav.DataSource.Internal.Query;

/// <summary>
/// Constants used by Queries / VisualQuery
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class QueryConstants
{
    #region Params


    /// <summary>
    /// A special property added to the `Params` source to find out if the current user can see drafts.
    /// </summary>
    [PrivateApi] public const string ParamsShowDraftsKey = "ShowDrafts";

    /// <summary>
    /// Default for `ShowDrafts` is `false`
    /// </summary>
    [PrivateApi] public const bool ParamsShowDraftsDefault = false;

    #endregion

    [PrivateApi] internal const string PartAssemblyAndType = "PartAssemblyAndType";

    [PrivateApi] internal const string VisualDesignerData = "VisualDesignerData";


    #region Query terms - used for DataSources but also the App project which has a Query Manager

    /// <summary>content type name of the query AttributeSet</summary>
    [PrivateApi] internal static readonly string QueryTypeName = "DataPipeline";

    /// <summary>content-type name of the queryPart AttributeSet</summary>
    [PrivateApi] internal static readonly string QueryPartTypeName = "DataPipelinePart";

    #endregion

    /// <summary>
    /// Attribute Name on the query-Entity describing the Stream-Wiring
    /// </summary>
    [PrivateApi] public const string QueryStreamWiringAttributeName = "StreamWiring";
}
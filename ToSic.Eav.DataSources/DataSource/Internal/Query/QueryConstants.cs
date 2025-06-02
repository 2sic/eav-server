namespace ToSic.Eav.DataSource.Internal.Query;

/// <summary>
/// Constants used by Queries / VisualQuery
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
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

    /// <summary>Content-Type name of the query Content-Type</summary>
    [PrivateApi] internal static readonly string QueryTypeName = "DataPipeline";

    /// <summary>Content-Type name of the queryPart Content-Type</summary>
    [PrivateApi] internal static readonly string QueryPartTypeName = "DataPipelinePart";

    #endregion

    /// <summary>
    /// Content-Type Name on the query-Entity describing the Stream-Wiring
    /// </summary>
    [PrivateApi] public const string QueryStreamWiringAttributeName = "StreamWiring";
}
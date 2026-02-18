namespace ToSic.Eav.DataSource.Query.Sys;

/// <summary>
/// Constants used by Queries / VisualQuery
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryConstants
{
    /// <summary>
    /// A special property added to the `Params` source to find out if the current user can see drafts.
    /// </summary>
    public const string ParamsShowDraftsKey = "ShowDrafts";

    /// <summary>
    /// Default for `ShowDrafts` is `false`
    /// </summary>
    public const bool ParamsShowDraftsDefault = false;

}
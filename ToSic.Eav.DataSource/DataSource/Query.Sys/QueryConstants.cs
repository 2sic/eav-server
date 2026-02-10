namespace ToSic.Eav.DataSource.Query.Sys;

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

}
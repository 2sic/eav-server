namespace ToSic.Sys.Security.Permissions;

/// <summary>
/// A wrapper for many permission checks. All of which must approve, for the request to succeed
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IMultiPermissionCheck: IHasLog
{
    /// <summary>
    /// All the permission checks that will be used
    /// </summary>
    public Dictionary<string, IPermissionCheck> PermissionCheckers { get; }
}
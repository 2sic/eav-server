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

    /// <summary>
    /// Verify that all checks pass
    /// </summary>
    /// <param name="grants"></param>
    /// <returns>True if all pass, false if any one fails</returns>
    bool UserMayOnAll(List<Grants> grants);

    /// <summary>
    /// Ensure that all! checks pass
    /// </summary>
    /// <param name="grants"></param>
    /// <param name="error">Out error message to use to throw upstream</param>
    /// <returns>True if all pass, false if any one fails</returns>
    bool EnsureAll(List<Grants> grants, out string? error);
}
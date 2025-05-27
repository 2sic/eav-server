namespace ToSic.Sys.Security.Permissions;

/// <summary>
/// A wrapper for many permission checks. All of which must approve, for the request to succeed
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IMultiPermissionCheck
{
    bool UserMayOnAll(List<Grants> grants);

    bool EnsureAll(List<Grants> grants, out string error);
}
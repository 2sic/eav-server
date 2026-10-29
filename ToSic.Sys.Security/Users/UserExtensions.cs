using ToSic.Sys.Users.Permissions;

namespace ToSic.Sys.Users;

public static class UserExtensions
{
    /// <summary>
    /// Check if the user may admin
    /// </summary>
    public static bool MayAdmin(this IUser user)
        => user != null! // Note: I'm not sure if the user could ever be null, but maybe in search scenarios?
           && (user.IsSystemAdmin || user.IsSiteAdmin || user.IsSiteDeveloper);

    /// <summary>
    /// Return a permissions object based on a user.
    /// </summary>
    public static EffectivePermissions EffectivePermissions(this IUser user)
    {
        var mayAdmin = user.MayAdmin();
        var isContentAdmin = user?.IsContentAdmin ?? false;
        var isContentEditor = user?.IsContentEditor ?? false;
        return new(
            IsSiteAdmin: mayAdmin,
            IsContentAdmin: mayAdmin || isContentAdmin,
            IsContentEditor: mayAdmin || isContentEditor,
            ShowDraftData: mayAdmin || isContentAdmin || isContentEditor
        );
    }
}

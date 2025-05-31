using System.Diagnostics.CodeAnalysis;
using ToSic.Lib.Services;

namespace ToSic.Sys.Users.Permissions;

/// <summary>
/// Very basic implementation.
/// Should be replaced in Eav.Apps which often knows more about the user than just the simple site context
/// </summary>
internal class CurrentContextUserPermissionsBasic(IUser user) : ServiceBase("Eav.CtxSec"), ICurrentContextUserPermissions
{
    [field: AllowNull, MaybeNull]
    EffectivePermissions ICurrentContextUserPermissions.Permissions => field ??= GetPermissions();

    private EffectivePermissions GetPermissions()
    {
        var userIsSiteAdmin = UserMayAdmin();
        var isContentAdmin = userIsSiteAdmin || (user?.IsContentAdmin ?? false);
        return new(isSiteAdmin: userIsSiteAdmin, isContentAdmin: isContentAdmin);
    }

    private bool UserMayAdmin()
    {
        var l = Log.Fn<bool>();
        if (user == null)
            return l.Return(false, "user unknown, false");
        return user.IsSystemAdmin 
            ? l.Return(true, "super")
            : l.Return(user.IsSiteAdmin || user.IsSiteDeveloper, "admin/developer");
    }


}
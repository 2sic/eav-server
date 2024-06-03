using ToSic.Lib.Services;

namespace ToSic.Eav.Context;

/// <summary>
/// Very basic implementation.
/// Should be replaced in Eav.Apps which often knows more about the user than just the simple site context
/// </summary>
internal class ContextOfUserPermissions(IUser user) : ServiceBase("Eav.CtxSec"), IContextOfUserPermissions
{
    AdminPermissions IContextOfUserPermissions.Permissions => _permissions ??= GetPermissions();
    private AdminPermissions _permissions;

    private AdminPermissions GetPermissions()
    {
        var userIsSiteAdmin = UserMayAdmin();
        var isContentAdmin = userIsSiteAdmin || (user?.IsContentAdmin ?? false);
        return new(isContentAdmin, userIsSiteAdmin);
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
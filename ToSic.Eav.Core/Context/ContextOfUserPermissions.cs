using ToSic.Eav.Plumbing;
using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Context;

/// <summary>
/// Very basic implementation.
/// Should be replaced in Eav.Apps which often knows more about the user than just the simple site context
/// </summary>
internal class ContextOfUserPermissions(IUser user) : ServiceBase("Eav.CtxSec"), IContextOfUserPermissions
{
    public IUser User { get; } = user;

    private bool UserMayAdmin()
    {
        var l = Log.Fn<bool>();
        var u = User;
        if (u == null) return l.Return(false, "user unknown, false");
        // Case 1: Superuser always may
        if (u.IsSystemAdmin) return l.Return(true, "super");

        return l.Return(u.IsSiteAdmin || u.IsSiteDeveloper, "admin/developer");
    }

    AdminPermissions IContextOfUserPermissions.Permissions => _permissions ??= UserMayAdmin().Map(userMay => new AdminPermissions(userMay || (User?.IsContentAdmin ?? false), userMay));
    private AdminPermissions _permissions;

}
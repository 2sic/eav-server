using ToSic.Sys.Users;
using ToSic.Sys.Utils;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Sys.Security.Permissions;

internal class EnvironmentPermissionUnknown(IUser user, WarnUseOfUnknown<EnvironmentPermissionUnknown> _)
    : ServiceBase(LogScopes.NotImplemented), IEnvironmentPermission
{
    public PermissionCheckInfo EnvironmentAllows(List<Grants> grants)
        => new(user.IsSystemAdmin, Conditions.EnvironmentGlobal);

    public bool VerifyConditionOfEnvironment(string condition) 
        => condition.EqualsInsensitive("SecurityAccessLevel.Anonymous");
}

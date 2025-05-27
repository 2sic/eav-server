using ToSic.Eav.Internal.Unknown;
using ToSic.Eav.Security;
using ToSic.Sys.Security.Permissions;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Integration.Security;

internal class EnvironmentPermissionUnknown(WarnUseOfUnknown<EnvironmentPermissionUnknown> _) : EnvironmentPermission(LogScopes.NotImplemented)
{
    protected override bool EnvironmentOk(List<Grants> grants) => UserIsSystemAdmin();

    public override bool VerifyConditionOfEnvironment(string condition) 
        => condition.Equals("SecurityAccessLevel.Anonymous", StringComparison.CurrentCultureIgnoreCase);
}
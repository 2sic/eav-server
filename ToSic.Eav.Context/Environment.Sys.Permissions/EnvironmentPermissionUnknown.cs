using ToSic.Sys.Security.Permissions;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Environment.Sys.Permissions;

internal class EnvironmentPermissionUnknown(WarnUseOfUnknown<EnvironmentPermissionUnknown> _) : EnvironmentPermission(LogScopes.NotImplemented)
{
    protected override bool EnvironmentOk(List<Grants> grants) => UserIsSystemAdmin();

    public override bool VerifyConditionOfEnvironment(string condition) 
        => condition.Equals("SecurityAccessLevel.Anonymous", StringComparison.CurrentCultureIgnoreCase);
}
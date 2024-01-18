using ToSic.Eav.Internal.Unknown;
using ToSic.Eav.Security;

namespace ToSic.Eav.Integration.Security;

internal class EnvironmentPermissionUnknown : EnvironmentPermission
{
    public EnvironmentPermissionUnknown(WarnUseOfUnknown<EnvironmentPermissionUnknown> _) : base(LogScopes.NotImplemented)
    { }

    public override bool EnvironmentAllows(List<Grants> grants) => UserIsSystemAdmin();

    public override bool VerifyConditionOfEnvironment(string condition) 
        => condition.Equals("SecurityAccessLevel.Anonymous", StringComparison.CurrentCultureIgnoreCase);
}
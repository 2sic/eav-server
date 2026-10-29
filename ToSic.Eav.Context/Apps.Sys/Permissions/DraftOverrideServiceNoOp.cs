using ToSic.Sys.Users.Permissions;

namespace ToSic.Eav.Apps.Sys.Permissions;

/// <summary>
/// No-Op draft override service, which will not change anything.
/// </summary>
public class DraftOverrideServiceNoOp: IDraftOverrideService
{
    public EffectivePermissions UserPermissions(EffectivePermissions permissions) => permissions;
}

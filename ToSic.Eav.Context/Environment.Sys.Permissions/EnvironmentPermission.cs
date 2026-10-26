using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Environment.Sys.Permissions;

/// <summary>
/// Basic implementation of the environment permissions.
/// Should normally be re-implemented by each specific platform.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class EnvironmentPermission() : ServiceWithSetup<EnvironmentPermissionOptions>($"Eav.EnvPrm"), IEnvironmentPermission
{
    public void Setup(EnvironmentPermissionOptionsBase options)
        => base.Setup(options as EnvironmentPermissionOptions ?? throw new InvalidCastException("Must convert the options, but not compatible."));

    /// <inheritdoc/>
    public PermissionCheckInfo EnvironmentAllows(List<Grants> grants)
    {
        var ok = new EnvironmentPermissionsLogic(parentLog: Log)
            .EnvironmentAllows(grants: grants, options: MyOptions, userIsModuleAdmin: () => false, userIsModuleEditor: () => false);
        return new(ok, ok ? Conditions.EnvironmentGlobal : Conditions.Undefined);
    }

    /// <inheritdoc/>
    public bool VerifyConditionOfEnvironment(string condition)
        => false;
}
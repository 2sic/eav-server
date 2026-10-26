using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.DI;
using ToSic.Sys.Users;

namespace ToSic.Sys.Security.Permissions;

public class PermissionCheckSysAdmin(Generator<PermissionCheck, PermissionCheck.Options> permCheck)
{
    /// <summary>
    /// Setup DI.
    /// </summary>
    public class Startup() : QuickStartup(s =>
    {
        s.TryAddTransient<IUser>(_ => new UserMock { IsSystemAdmin = true });
        s.SetupSysSecurity();
    });


    private PermissionCheckInfo CheckRead() =>
        permCheck.New(new(Permissions: [], TargetOwner: "", User: null, Env: null))
            .UserMay([Grants.Read]);


    [Fact]
    public void SuperUserIsAllowed() => True(CheckRead().Allowed);

    [Fact]
    public void SuperUserReasonIsEnvGlobal() => Equal(Conditions.EnvironmentGlobal, CheckRead().Condition);

}
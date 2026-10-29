using ToSic.Sys.DI;

namespace ToSic.Sys.Security.Permissions;

public class PermissionCheckBasics(Generator<PermissionCheck, PermissionCheck.Options> permCheck)
{
    /// <summary>
    /// Setup DI.
    /// </summary>
    public class Startup() : QuickStartup(s => s.SetupSysSecurity());

    [Fact]
    public void Create()
        => NotNull(permCheck.New(new(Permissions: [], TargetOwner: "", User: null, Env: null)));

    [Fact]
    public void NotAllowedByDefault() => False(
        permCheck.New(new(Permissions: [], TargetOwner: "", User: null, Env: null))
            .UserMay([Grants.Read]).Allowed
    );
}
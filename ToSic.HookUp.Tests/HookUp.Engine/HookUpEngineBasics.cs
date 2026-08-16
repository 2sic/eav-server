using ToSic.Sys;
using ToSic.Sys.HookUp;
using ToSic.Sys.Run.Startup;

namespace ToSic.HookUp.Engine;

public class HookUpEngineBasics(IHookUp hookUp)
{
    public class Startup() : QuickStartup(s => s.AddHookUp());

    [Fact]
    public void HookUp_NotNull()
        => NotNull(hookUp);

    [Fact]
    public void HookUp_StartsWith_NotNull()
        => NotNull(hookUp.StartWith(7));

    [Fact]
    public void HookUp_StartsWith_PackageNotNull()
        => NotNull(hookUp.StartWith(7).Package);

    [Fact]
    public void HookUp_StartsWith_PackageContains()
        => Equal(7, hookUp.StartWith(7).Package.Data);
}
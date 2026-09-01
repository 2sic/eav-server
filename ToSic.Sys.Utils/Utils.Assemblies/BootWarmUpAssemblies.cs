using ToSic.Sys.Boot;

namespace ToSic.Sys.Utils.Assemblies;

/// <summary>
/// Pre-Load the Assembly list into memory to log separately, and to ensure that the time needed is not mixed with other processes.
/// </summary>
/// <param name="logStore"></param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public sealed class BootWarmUpAssemblies(ILogStore logStore)
    : BootProcessBase("SqlWUp", bootPhase: BootPhase.WarmUp)
{
    /// <summary>
    /// This will index the assemblies and put them in a list for future/faster lookup.
    /// </summary>
    public override void Run()
    {
        var (_, lStandalone, lNormal) = BootLogHelper
            .GetLoggersForStandaloneLogs(logStore, Log, "AssLdr", "Load Assemblies");
        
        AssemblyHandling.GetTypes(lStandalone);
        lStandalone.Done();
        lNormal.Done();
    }

}
using ToSic.Eav.Plumbing;
using ToSic.Eav.StartUp;
using ToSic.Lib.Boot;

namespace ToSic.Sys.Utils.Assembly;

/// <summary>
/// Pre-Load the Assembly list into memory to log separately.
/// </summary>
/// <remarks>
/// This is in the library/sys namespace, but it's not registered for auto-run in this DLL,
/// because the consuming DLL should decide if it wants to run this or not.
/// </remarks>
/// <param name="logStore"></param>
public class BootWarmUpAssemblies(ILogStore logStore)
    : BootProcessBase("SqlWUp", bootPhase: BootPhase.WarmUp)
{
    /// <summary>
    /// This will access the DB once to warm up the connection pool and make sure the DB is available.
    /// </summary>
    public override void Run()
    {
        var (_, lStandalone, lNormal) = BootLogHelper.GetLoggersForStandaloneLogs(logStore, Log, "AssLdr", "Load Assemblies");
        AssemblyHandling.GetTypes(lStandalone);
        lStandalone.Done();
        lNormal.Done();
    }

}
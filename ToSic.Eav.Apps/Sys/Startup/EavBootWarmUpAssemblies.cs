using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Boot;

namespace ToSic.Eav.StartUp;

/// <summary>
/// Pre-Load the Assembly list into memory to log separately
/// </summary>
/// <param name="logStore"></param>
/// <param name="typeSvc"></param>
internal class EavBootWarmUpAssemblies(ILogStore logStore, LazySvc<ITargetTypeService> typeSvc)
    : BootProcessBase("SqlWUp", bootPhase: BootPhase.WarmUp, connect: [typeSvc])
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
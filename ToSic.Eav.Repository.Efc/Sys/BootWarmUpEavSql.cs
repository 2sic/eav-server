using ToSic.Eav.Metadata.Targets;
using ToSic.Sys.Boot;

namespace ToSic.Eav.Repository.Efc.Sys;

/// <summary>
/// Do initial access to the DB so it's faster later
/// </summary>
/// <param name="logStore"></param>
/// <param name="typeSvc"></param>
internal sealed class BootWarmUpEavSql(ILogStore logStore, LazySvc<ITargetTypeService> typeSvc)
    : ServiceBase("SqlWUp", connect: [logStore, typeSvc]), IBootProcess
{

    public string NameId => Log.NameId;

    public BootPhase Phase => BootPhase.Registrations;

    public int Priority => 999;

    /// <summary>
    /// This will access the DB once to warm up the connection pool and make sure the DB is available.
    /// </summary>
    public void Run()
    {
        var (main, lStandalone, lNormal) = BootLogHelper.GetLoggersForStandaloneLogs(logStore, Log, "SqlWUp", "Warm up SQL");
        typeSvc.LinkLog(main).Value.GetName(1);
        lStandalone.Done();
        lNormal.Done();
    }

}
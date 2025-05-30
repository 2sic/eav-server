using ToSic.Lib.Services;

namespace ToSic.Lib.Boot;

/// <summary>
/// The main / overall loader which will coordinate all the loaders.
/// </summary>
/// <remarks>
/// It will run pre-loaders first, then the main loader.
/// </remarks>
public class BootCoordinator : ServiceBase
{
    public BootCoordinator(ILogStore logStore, IEnumerable<IBootProcess> bootProcesses)
        : base("B8T.SysLdr", connect: [bootProcesses])
    {
        logStore.Add(LogNames.LogStoreStartUp, Log);
        Log.A("B8T - Boot System Loader");
        _bootProcesses = bootProcesses;
    }

    private readonly IEnumerable<IBootProcess> _bootProcesses;

    public void StartUp()
    {
        var l = Log.Fn(timer: true);
        BootAllProcesses();
        l.Done();
    }

    private void BootAllProcesses()
    {
        var l = Log.Fn($"{_bootProcesses.Count()} Processes");

        // Order by Phase
        var ordered = _bootProcesses
            .OrderBy(p => p.Phase)
             .ThenBy(p => p.Priority)
            .ToList();
        //var ordered = _bootProcesses
        //    .OrderBy(p => (int)p.Phase * 10000 + p.Priority)
        //    // .ThenBy(p => p.Priority)
        //    .ToList();

        foreach (var registration in ordered)
            BootOneProcess(registration);
        l.Done();
    }

    private void BootOneProcess(IBootProcess bootProcess)
    {
        var l = Log.Fn($"{bootProcess.NameId}; Phase: {bootProcess.Phase.ToString()}");
        try
        {
            // TODO: to remove this init, we need to implement something in the ConnectService #dropLogInit
            // which can handle DI-IEnumerables. To dev this we would need unit tests
            bootProcess.LinkLog(Log);
            bootProcess.Run();
        }
        catch (Exception ex)
        {
            l.A($"Error on registration of {bootProcess.NameId}");
            l.Ex(ex);
        }
        l.Done();
    }
}
namespace ToSic.Sys.Boot;

/// <summary>
/// During boot, this will get all <see cref="IBootProcess"/> services and ensure that they are triggered in the correct order.
/// </summary>
/// <remarks>
/// It will run pre-loaders first, then the main loader.
/// Loaders are registered in the Dependency Injection.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class BootCoordinator : ServiceBase
{
    /// <summary>
    /// Constructor, don't call, use DI.
    /// </summary>
    public BootCoordinator(ILogStore logStore, IEnumerable<IBootProcess> bootProcesses)
        : base("B8T.SysLdr", connect: [bootProcesses])
    {
        logStore.Add(LogNames.LogStoreStartUp, Log);
        Log.A("B8T - Boot System Loader");
        _bootProcesses = bootProcesses;
    }

    private readonly IEnumerable<IBootProcess> _bootProcesses;

    /// <summary>
    /// This should be called by the runtime environment upon start.
    /// </summary>
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
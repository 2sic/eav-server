using ToSic.Eav.StartUp;

namespace ToSic.Eav.Internal.Loaders;

/// <summary>
/// The main / overall loader which will coordinate all the loaders.
/// </summary>
/// <remarks>
/// It will run pre-loaders first, then the main loader.
/// </remarks>
public class SystemLoader : ServiceBase
{
    public SystemLoader(
        ILogStore logStore,
        IEnumerable<IStartUpRegistrations> startUpRegistrations,
        LazySvc<EavSystemLoader> systemLoaderLazy // This must be lazy, as some dependencies of it could change till it's needed
    ) : base($"{EavLogs.Eav}SysLdr", connect: [startUpRegistrations, systemLoaderLazy])
    {
        logStore.Add(LogNames.LogStoreStartUp, Log);
        Log.A("EAV System Loader");
        _startUpRegistrations = startUpRegistrations;
        _systemLoaderLazy = systemLoaderLazy;
    }

    private readonly IEnumerable<IStartUpRegistrations> _startUpRegistrations;
    private readonly LazySvc<EavSystemLoader> _systemLoaderLazy;

    public void StartUp()
    {
        var l = Log.Fn();
        StartUpAllRegistrations();
        l.A("Will now run StartUp on EAV SystemLoader - logs are tracked separately");
        _systemLoaderLazy.Value.StartUp();
        l.Done();
    }

    private void StartUpAllRegistrations()
    {
        var l = Log.Fn();
        foreach (var registration in _startUpRegistrations)
            StartUpRegistration(registration);
        l.Done();
    }

    private void StartUpRegistration(IStartUpRegistrations registration)
    {
        var l = Log.Fn(registration.NameId);
        try
        {
            // TODO: to remove this init, we need to implement something in the ConnectService #dropLogInit
            // which can handle DI-IEnumerables. To dev this we would need unit tests
            registration.LinkLog(Log);
            registration.Register();
        }
        catch (Exception ex)
        {
            l.A($"Error on registration of {registration.NameId}");
            l.Ex(ex);
        }
        l.Done();
    }
}
using ToSic.Eav.StartUp;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Loaders;

/// <summary>
/// WIP - the main loader which will run pre-loaders first, then the main loader
/// </summary>
public class SystemLoader : ServiceBase
{
    public SystemLoader(
        ILogStore logStore,
        IEnumerable<IStartUpRegistrations> startUpRegistrations,
        LazySvc<EavSystemLoader> systemLoaderLazy // This must be lazy, as some dependencies of it could change till it's needed
    ) : base($"{EavLogs.Eav}SysLdr")
    {
        logStore.Add(LogNames.LogStoreStartUp, Log);
        Log.A("EAV System Loader");
        ConnectLogs([
            _startUpRegistrations = startUpRegistrations,
            _systemLoaderLazy = systemLoaderLazy
        ]);
    }

    private readonly IEnumerable<IStartUpRegistrations> _startUpRegistrations;
    private readonly LazySvc<EavSystemLoader> _systemLoaderLazy;

    /// <summary>
    /// This is just for public access, don't use in this file
    /// </summary>
    public EavSystemLoader EavSystemLoader => _systemLoaderLazy.IsValueCreated
        ? _systemLoaderLazy.Value
        : throw new("Can't access this property unless StartUp has run first");

    public void StartUp()
    {
        var l = Log.Fn();
        DoRegistrations();
        l.A("Will now run StartUp on EAV SystemLoader - logs are tracked separately");
        _systemLoaderLazy.Value.StartUp();
        l.Done();
    }

    private void DoRegistrations()
    {
        var l = Log.Fn();
        foreach (var registration in _startUpRegistrations)
            DoRegistration(registration);
        l.Done();
    }

    private void DoRegistration(IStartUpRegistrations registration)
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
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.Loaders;

namespace ToSic.Eav.Work.Tests.ConditionalDraftSql;

public class StartupConditionalDraftSqlWork : StartupTestWork
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        var native = services.Single(s => s.ServiceType == typeof(IAppsCacheSwitchable) && s.ImplementationType?.Name == "AppsCache");
        services.Remove(native);
        services.AddSingleton(sp => new ConditionalDraftSqlCache((IAppsCacheSwitchable)ActivatorUtilities.CreateInstance(sp, native.ImplementationType!)));
        services.AddSingleton<IAppsCacheSwitchable>(sp => sp.GetRequiredService<ConditionalDraftSqlCache>());
    }
}

// Observe the real native cache; all reads, invalidations and updates still delegate to it.
public sealed class ConditionalDraftSqlCache(IAppsCacheSwitchable native) : IAppsCacheSwitchable
{
    public string NameId => "C3ObservedNativeCache";
    public int Priority => native.Priority;
    public bool IsViable() => native.IsViable();
    public Action? BeforeUpdate { get; set; }
    public int Updates { get; private set; }
    public int Purges { get; private set; }
    public void Reset() { BeforeUpdate = null; Updates = 0; Purges = 0; }
    public IAppStateCache Get(IAppIdentity app, IAppLoaderTools tools) => native.Get(app, tools);
    public IReadOnlyDictionary<int, Zone> Zones(IAppLoaderTools tools) => native.Zones(tools);
    public int ZoneIdOfApp(int appId, IAppLoaderTools tools) => native.ZoneIdOfApp(appId, tools);
    public bool Has(IAppIdentity app) => native.Has(app);
    public void Purge(IAppIdentity app) { Purges++; native.Purge(app); }
    public void PurgeZones() => native.PurgeZones();
    public void Add(IAppStateCache appState) => native.Add(appState);
    public void Load(IAppIdentity app, string language, IAppLoaderTools tools) => native.Load(app, language, tools);
    public void Update(IAppIdentity app, IEnumerable<int> entities, ILog log, IAppLoaderTools tools)
    {
        BeforeUpdate?.Invoke();
        native.Update(app, entities, log, tools);
        Updates++;
    }
}

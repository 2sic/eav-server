using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Lib.Coding;
using static ToSic.Eav.Apps.Sys.KnownAppsConstants;

namespace ToSic.Eav.Apps.AppReader.Sys;

internal class AppReaderFactory(LazySvc<IAppsCatalog> appsCatalog, IAppStateCacheService appStates, Generator<AppReader> readerGenerator)
    : ServiceBase("Eav.AppRds"), IAppReaderFactory
{
    public IAppReader? GetOrKeep(IAppIdentity appOrReader)
        => appOrReader as IAppReader ?? TryGet(appOrReader);

    public IAppReader? TryGet(IAppIdentity appIdentity)
    {
        var l = Log.Fn<IAppReader?>(appIdentity.Show());
        var state = appStates.Get(appIdentity);
        return l.ReturnAndLogIfNull(ToReader(state));
    }
    public IAppReader Get(IAppIdentity appIdentity)
    {
        var l = Log.Fn<IAppReader?>(appIdentity.Show());
        var result = TryGet(appIdentity);
        if (result == null)
            throw new NullReferenceException($"App '{appIdentity.Show()}' not found in cache, so it can't be read. This is a bug, please report it to the 2sxc team.");
        return l.Return(result);
    }

    //public IAppReader? TryGet(int appId)
    //{
    //    var l = Log.Fn<IAppReader?>($"{appId}");
    //    var state = appStates.Get(appId);
    //    return l.ReturnAndLogIfNull(ToReader(state));
    //}

    public IAppReader Get(int appId)
    {
        var l = Log.Fn<IAppReader?>($"{appId}");
        var state = appStates.Get(appId);
        if (state == null)
            throw new NullReferenceException($"App '{appId}' not found in cache, so it can't be read. This is a bug, please report it to the 2sxc team.");
        return l.Return(ToReader(state));
    }

    public IAppReader GetZonePrimary(int zoneId)
    {
        var primaryAppId = appsCatalog.Value.PrimaryAppIdentity(zoneId);
        return Get(primaryAppId);
    }

    public IAppIdentityPure AppIdentity(int appId)
        => appsCatalog.Value.AppIdentity(appId);

    public IAppReader GetSystemPreset()
        => Get(PresetIdentity);

    public IAppReader? GetSystemPreset(NoParamOrder protector = default, bool nullIfNotLoaded = false)
    {
        if (nullIfNotLoaded && !((AppStateCacheService)appStates).AppsCacheSwitch.Value.Has(PresetIdentity)) 
            return null;
        return GetSystemPreset();
    }

    [return: NotNullIfNotNull(nameof(state))]
    public IAppReader? ToReader(IAppStateCache state)
#pragma warning disable CS8825 // Return value must be non-null because parameter is non-null.
        => state is AppState typed
            ? readerGenerator.New().Init(typed, parentLog: null)
            : null;
#pragma warning restore CS8825 // Return value must be non-null because parameter is non-null.
}
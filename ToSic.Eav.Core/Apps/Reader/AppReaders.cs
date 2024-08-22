using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.State;
using ToSic.Lib.Coding;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using static ToSic.Eav.Constants;

namespace ToSic.Eav.Apps;

internal class AppReaders(LazySvc<IAppsCatalog> appsCatalog, IAppStateCacheService appStates, Generator<AppReader> readerGenerator)
    : ServiceBase("Eav.AppRds"), IAppReaders
{
    public IAppReader GetOrKeep(IAppIdentity appOrReader)
        => appOrReader as IAppReader ?? Get(appOrReader);

    public IAppReader Get(IAppIdentity app)
    {
        var state = appStates.Get(app);
        return state is null ? null : ToReader(state);
    }

    public IAppReader Get(int appId)
    {
        var state = appStates.Get(appId);
        return state is null ? null : ToReader(state);
    }

    public IAppReader GetZonePrimary(int zoneId)
    {
        var primaryAppId = appsCatalog.Value.PrimaryAppIdentity(zoneId);
        return Get(primaryAppId);
    }

    public IAppReader GetSystemPreset(NoParamOrder protector = default, bool nullIfNotLoaded = false)
    {
        if (nullIfNotLoaded && !((AppStateCacheService)appStates).AppsCacheSwitch.Value.Has(PresetIdentity)) 
            return null;
        return Get(PresetIdentity);
    }

    public IAppReader ToReader(IAppStateCache state)
        => readerGenerator.New().Init(state, parentLog: null);
}
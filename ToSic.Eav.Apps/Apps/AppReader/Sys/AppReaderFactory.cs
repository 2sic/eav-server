using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.State;
using ToSic.Lib.Coding;
using static ToSic.Eav.Apps.Sys.KnownAppsConstants;

namespace ToSic.Eav.Apps;

internal class AppReaderFactory(LazySvc<IAppsCatalog> appsCatalog, IAppStateCacheService appStates, Generator<AppReader> readerGenerator)
    : ServiceBase("Eav.AppRds"), IAppReaderFactory
{
    public IAppReader GetOrKeep(IAppIdentity appOrReader)
        => appOrReader as IAppReader ?? Get(appOrReader);

    public IAppReader Get(IAppIdentity app)
    {
        var l = Log.Fn<IAppReader>(app.Show());
        var state = appStates.Get(app);
        return l.ReturnAndLogIfNull(ToReader(state));
    }

    public IAppReader Get(int appId)
    {
        var l = Log.Fn<IAppReader>($"{appId}");
        var state = appStates.Get(appId);
        return l.ReturnAndLogIfNull(ToReader(state));
    }

    public IAppReader GetZonePrimary(int zoneId)
    {
        var primaryAppId = appsCatalog.Value.PrimaryAppIdentity(zoneId);
        return Get(primaryAppId);
    }

    public IAppIdentityPure AppIdentity(int appId) => appsCatalog.Value.AppIdentity(appId);

    public IAppReader GetSystemPreset(NoParamOrder protector = default, bool nullIfNotLoaded = false)
    {
        if (nullIfNotLoaded && !((AppStateCacheService)appStates).AppsCacheSwitch.Value.Has(PresetIdentity)) 
            return null;
        return Get(PresetIdentity);
    }

    public IAppReader ToReader(IAppStateCache state) =>
        state == null
            ? null
            : readerGenerator.New().Init(state, parentLog: null);
}
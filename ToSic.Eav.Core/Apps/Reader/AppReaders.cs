using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Apps.State;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using static ToSic.Eav.Constants;

namespace ToSic.Eav.Apps;

internal class AppReaders(LazySvc<IAppsCatalog> appsCatalog, IAppStateCacheService appStates, Generator<AppReader> readerGenerator)
    : ServiceBase("Eav.AppRds"), IAppReaders
{
    public IAppSpecs GetAppSpecs(int appId)
        => (appStates.Get(appId) as IHas<IAppSpecs>).Value;

    public IAppContentTypeService GetContentTypes(IAppIdentity app)
        => GetReader(app);

    public IAppContentTypeService GetContentTypes(int appId)
        => GetReader(appId);

    public IAppContentTypeService GetPresetReaderIfAlreadyLoaded()
        => (appStates as AppStateCacheService)?.AppsCacheSwitch.Value.Has(PresetIdentity) ?? false
            ? GetPresetReader()
            : null;

    public IAppReader KeepOrGetReader(IAppIdentity app)
        => app as IAppReader
            ?? (app is IAppStateCache stateCache ? ToReader(stateCache) : null)
           ?? GetReader(app);

    public IAppReader GetReader(IAppIdentity app)
    {
        var state = appStates.Get(app);
        return state is null ? null : ToReader(state);
    }

    public IAppReader GetReader(int appId)
    {
        var state = appStates.Get(appId);
        return state is null ? null : ToReader(state);
    }

    public IAppReader GetPrimaryReader(int zoneId)
    {
        var primaryAppId = appsCatalog.Value.PrimaryAppIdentity(zoneId);
        return GetReader(primaryAppId);
    }

    public IAppReader GetPresetReader()
        => GetReader(PresetIdentity);

    public IAppReader ToReader(IAppStateCache state) //, ILog log = default)
        => readerGenerator.New().Init(state, parentLog: null);
}
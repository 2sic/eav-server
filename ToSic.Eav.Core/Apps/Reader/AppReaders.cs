using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Apps.State;
using ToSic.Lib.DI;
using static ToSic.Eav.Constants;

namespace ToSic.Eav.Apps;

internal class AppReaders(IAppStates appStates, Generator<AppReader> readerGenerator) : IAppReaders
{
    public IAppsCatalog AppsCatalog => appStates.AppsCatalog;

    public IAppSpecs GetAppSpecs(int appId)
        => (appStates.GetCacheState(appId) as IHas<IAppSpecs>).Value;

    public IAppSpecsWithState GetAppSpecsWithState(int appId)
        => (appStates.GetCacheState(appId) as IHas<IAppSpecsWithState>).Value;


    public IAppContentTypeService GetContentTypes(IAppIdentity app)
        => GetReader(app);

    public IAppContentTypeService GetContentTypes(int appId)
        => GetReader(appId);

    public IAppContentTypeService GetPresetReaderIfAlreadyLoaded()
        => (appStates as AppStates)?.AppsCacheSwitch.Value.Has(PresetIdentity) ?? false
            ? GetPresetReader()
            : null;

    public IAppReader KeepOrGetReader(IAppIdentity app)
        => app as IAppReader
            ?? (app is IAppStateCache stateCache ? ToReader(stateCache) : null)
           ?? GetReader(app);

    public IAppReader GetReader(IAppIdentity app, ILog log = default)
    {
        var state = appStates.Get(app);
        return state is null ? null : ToReader(state, log);
    }

    public IAppReader GetReader(int appId, ILog log = default)
    {
        var state = appStates.GetCacheState(appId);
        return state is null ? null : ToReader(state, log);
    }

    public IAppReader GetPrimaryReader(int zoneId, ILog log)
    {
        var l = log.Fn<IAppReader>($"{zoneId}");
        var primaryAppId = appStates.AppsCatalog.PrimaryAppIdentity(zoneId);
        return l.Return(GetReader(primaryAppId, log), primaryAppId.Show());
    }

    public IAppReader GetPresetReader()
        => GetReader(PresetIdentity);

    public IAppReader ToReader(IAppStateCache state, ILog log = default)
        => readerGenerator.New().Init(state, log);
}
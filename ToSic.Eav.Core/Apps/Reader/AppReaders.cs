using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Apps.State;
using ToSic.Lib.DI;
using static ToSic.Eav.Constants;

namespace ToSic.Eav.Apps;

internal class AppReaders(IAppStates appStates, Generator<AppStateDataService> readerGenerator) : IAppReaders
{
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

    public IAppStateInternal KeepOrGetReader(IAppIdentity app)
        => app as IAppStateInternal
            ?? (app is IAppStateCache stateCache ? ToReader(stateCache) : null)
           ?? GetReader(app);

    public IAppStateInternal GetReader(IAppIdentity app, ILog log = default)
    {
        var state = appStates.Get(app);
        return state is null ? null : ToReader(state, log);
    }

    public IAppStateInternal GetReader(int appId, ILog log = default)
    {
        var state = appStates.GetCacheState(appId);
        return state is null ? null : ToReader(state, log);
    }

    public IAppStateInternal GetPrimaryReader(int zoneId, ILog log)
    {
        var l = log.Fn<IAppStateInternal>($"{zoneId}");
        var primaryAppId = appStates.IdentityOfPrimary(zoneId);
        return l.Return(GetReader(primaryAppId, log), primaryAppId.Show());
    }

    public IAppStateInternal GetPresetReader()
        => GetReader(PresetIdentity);


    // todo: move the to-reader code here once we've changed all access
    public IAppStateInternal ToReader(IAppStateCache state, ILog log = default)
        => readerGenerator.New().Init(state, log);
}
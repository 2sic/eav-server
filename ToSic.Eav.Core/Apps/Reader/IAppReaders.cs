using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Apps;

public interface IAppReaders
{
    IAppSpecs GetAppSpecs(int appId);

    IAppSpecsWithState GetAppSpecsWithState(int appId);


    IAppContentTypeService GetContentTypes(IAppIdentity app);

    IAppContentTypeService GetContentTypes(int appId);

    IAppContentTypeService GetPresetReaderIfAlreadyLoaded();

    IAppStateInternal KeepOrGetReader(IAppIdentity app);

    IAppStateInternal GetReader(IAppIdentity app, ILog log = default);

    IAppStateInternal GetReader(int appId, ILog log = default);

    IAppStateInternal GetPrimaryReader(int zoneId, ILog log);

    IAppStateInternal GetPresetReader();

    IAppStateInternal ToReader(IAppStateCache state, ILog log = default);

}
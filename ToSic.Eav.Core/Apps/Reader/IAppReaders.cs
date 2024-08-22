using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Apps;

public interface IAppReaders
{
    IAppsCatalog AppsCatalog { get; }
    IAppSpecs GetAppSpecs(int appId);

    IAppSpecsWithState GetAppSpecsWithState(int appId);


    IAppContentTypeService GetContentTypes(IAppIdentity app);

    IAppContentTypeService GetContentTypes(int appId);

    IAppContentTypeService GetPresetReaderIfAlreadyLoaded();

    IAppReader KeepOrGetReader(IAppIdentity app);

    IAppReader GetReader(IAppIdentity app, ILog log = default);

    IAppReader GetReader(int appId, ILog log = default);

    IAppReader GetPrimaryReader(int zoneId, ILog log);

    IAppReader GetPresetReader();

    IAppReader ToReader(IAppStateCache state, ILog log = default);

}
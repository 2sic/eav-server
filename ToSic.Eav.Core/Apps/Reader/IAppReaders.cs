using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Apps;

public interface IAppReaders
{
    IAppSpecs GetAppSpecs(int appId);

    IAppContentTypeService GetContentTypes(IAppIdentity app);

    IAppContentTypeService GetContentTypes(int appId);

    IAppContentTypeService GetPresetReaderIfAlreadyLoaded();

    IAppReader KeepOrGetReader(IAppIdentity app);

    IAppReader GetReader(IAppIdentity app);

    IAppReader GetReader(int appId);

    IAppReader GetPrimaryReader(int zoneId);

    IAppReader GetPresetReader();

    IAppReader ToReader(IAppStateCache state);
}
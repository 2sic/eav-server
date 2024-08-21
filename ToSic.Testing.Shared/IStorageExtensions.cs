using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Repositories;

namespace ToSic.Testing.Shared;

public static  class IStorageExtensions
{
    public static IAppReader AppStateReaderRawTA(this IRepositoryLoader storage, int appId)
        => storage.AppStateBuilderRaw(appId, new()).Reader;
}
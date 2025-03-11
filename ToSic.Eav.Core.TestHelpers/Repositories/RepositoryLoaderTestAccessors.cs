using ToSic.Eav.Apps;

namespace ToSic.Eav.Repositories;

public static  class RepositoryLoaderTestAccessors
{
    public static IAppReader AppStateReaderRawTac(this IRepositoryLoader storage, int appId)
        => storage.AppStateBuilderRaw(appId, new()).Reader;
}
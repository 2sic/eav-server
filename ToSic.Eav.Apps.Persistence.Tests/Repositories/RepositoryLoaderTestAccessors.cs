using ToSic.Eav.Apps;

namespace ToSic.Eav.Repositories;

public static  class RepositoryLoaderTestAccessors
{
    public static IAppReader AppStateReaderRawTac(this IRepositoryLoaderWithRaw storage, int appId)
        => storage.AppReaderRaw(appId, new());
}
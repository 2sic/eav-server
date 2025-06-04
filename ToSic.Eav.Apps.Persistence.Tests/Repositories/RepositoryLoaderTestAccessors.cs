using ToSic.Eav.Apps;

namespace ToSic.Eav.Repositories;

public static  class RepositoryLoaderTestAccessors
{
    public static IAppReader AppStateReaderRawTac(this IAppsAndZonesLoaderWithRaw storage, int appId)
        => storage.AppReaderRaw(appId, new());
}
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys.Loaders;

namespace ToSic.Eav.Repositories;

public static  class RepositoryLoaderTestAccessors
{
    public static IAppReader AppStateReaderRawTac(this IAppsAndZonesLoaderWithRaw storage, int appId)
        => storage.AppReaderRaw(appId, new());
}
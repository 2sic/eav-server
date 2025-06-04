using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Caching;

/// <summary>
/// Tools for loading Apps from repositories.
/// It's used by the cache to load apps from the repository.
///
/// This level of indirection is necessary, because the cache is a singleton and does not have DI,
/// so it needs to be handed this object for certain operations.
/// </summary>
/// <param name="repoFactory"></param>
internal class AppLoaderTools(Generator<IAppsAndZonesLoader> repoFactory)
    : ServiceBase("Eav.LodTls", connect: [repoFactory]), IAppLoaderTools
{
    public IAppsAndZonesLoader RepositoryLoader(ILog parentLog)
        => repoFactory.New().LinkLog(parentLog ?? Log);
}
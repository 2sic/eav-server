using ToSic.Eav.Repositories;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Caching;

/// <summary>
/// Tools for loading Apps from repositories.
/// It's used by the cache to load apps from the repository.
///
/// This level of indirection is necessary, because the cache is a singleton and does not have DI,
/// so it needs to be handed this object for certain operations.
/// </summary>
/// <param name="repoFactory"></param>
internal class AppLoaderTools(Generator<IRepositoryLoader> repoFactory)
    : ServiceBase("Eav.LodTls", connect: [repoFactory]), IAppLoaderTools
{
    public IRepositoryLoader RepositoryLoader(ILog parentLog) => repoFactory.New().LinkLog(parentLog ?? Log);
}
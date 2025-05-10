using ToSic.Eav.Repositories;

namespace ToSic.Eav.Caching;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppLoaderTools
{
    IRepositoryLoader RepositoryLoader(ILog parentLog);
}
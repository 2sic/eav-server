using ToSic.Eav.Repositories;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Caching;

public interface IAppLoaderTools
{
    IRepositoryLoader RepositoryLoader(ILog parentLog);
}
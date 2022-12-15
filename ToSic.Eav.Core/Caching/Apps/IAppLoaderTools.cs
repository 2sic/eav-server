using ToSic.Eav.Repositories;

namespace ToSic.Eav.Caching
{
    public interface IAppLoaderTools
    {
        IRepositoryLoader RepositoryLoader { get; }
    }
}

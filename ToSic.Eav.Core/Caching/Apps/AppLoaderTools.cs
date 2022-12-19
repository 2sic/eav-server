using ToSic.Eav.Repositories;
using ToSic.Lib.DI;

namespace ToSic.Eav.Caching
{
    internal class AppLoaderTools: IAppLoaderTools
    {

        public AppLoaderTools(Generator<IRepositoryLoader> _repoFactory)
        {
            this._repoFactory = _repoFactory;
        }
        private readonly Generator<IRepositoryLoader> _repoFactory;

        
        public IRepositoryLoader RepositoryLoader => _repoFactory.New();
    }
}

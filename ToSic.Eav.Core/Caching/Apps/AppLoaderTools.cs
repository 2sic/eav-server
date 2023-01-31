using ToSic.Eav.Repositories;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Caching
{
    internal class AppLoaderTools: ServiceBase, IAppLoaderTools
    {

        public AppLoaderTools(Generator<IRepositoryLoader> repoFactory): base("Eav.LodTls")
        {
            ConnectServices(
                _repoFactory = repoFactory
            );
        }
        private readonly Generator<IRepositoryLoader> _repoFactory;

        
        public IRepositoryLoader RepositoryLoader(ILog parentLog) => _repoFactory.New().LinkLog(parentLog);
    }
}

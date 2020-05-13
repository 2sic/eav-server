using ToSic.Eav.Logging;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Run
{
    public interface IRuntimeFactory
    {
        // experimental

        IAppRepositoryLoader AppRepositoryLoader(int appId, string path, ILog log);

    }
}

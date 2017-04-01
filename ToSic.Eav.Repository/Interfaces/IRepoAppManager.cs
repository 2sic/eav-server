using System.Collections.Generic;

namespace ToSic.Eav.Repository.Interfaces
{
    public interface IRepoAppManager
    {
        void Delete(int appId);

        IRepoApp Add(IRepoZone zone, string name = Constants.DefaultAppName);

        List<IRepoApp> GetAll();

        IRepoApp GetOne(int appId);
    }
}

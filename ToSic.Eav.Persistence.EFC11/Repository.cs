using ToSic.Eav.Persistence.EFC11.Models;
using ToSic.Eav.Repository.Interfaces;

namespace ToSic.Eav.Persistence.EFC11
{
    public class Repository
    {
        public IRepoZoneManager Zones;
        public IRepoAppManager Apps;
        
        public EavDbContext Context;
        public Repository()
        {
            Context = new EavDbContext();

            Zones = new Repo.ZoneManager(Context);
        }
    }
}

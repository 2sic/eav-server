using System;
using System.Collections.Generic;
using System.Text;

namespace ToSic.Eav.Repository.Interfaces
{
    public interface IRepoZoneManager
    {
        void Delete(int zoneId);

        IRepoZone Create(string name);

        IRepoZone GetOne(int zoneId);

        IEnumerable<IRepoZone> GetAll();

        void Update(IRepoZone zone, string name);

    }
}

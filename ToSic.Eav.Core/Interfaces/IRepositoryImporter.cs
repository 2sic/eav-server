using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.Interfaces
{
    public interface IRepositoryImporter
    {
        void Import(int? zoneId, int appId, IEnumerable<ContentType> newAttributeSets, IEnumerable<Entity> newEntities);
    }
}

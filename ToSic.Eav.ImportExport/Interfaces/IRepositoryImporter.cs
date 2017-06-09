using System.Collections.Generic;
using ToSic.Eav.ImportExport.Models;

namespace ToSic.Eav.ImportExport.Interfaces
{
    public interface IRepositoryImporter
    {
        void Import(int? zoneId, int? appId, IEnumerable<ImpContentType> newAttributeSets, IEnumerable<ImpEntity> newEntities);
    }
}

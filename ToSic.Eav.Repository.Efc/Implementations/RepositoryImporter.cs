using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Repository.Efc.Parts;

namespace ToSic.Eav.Repository.Efc.Implementations
{
    /// <summary>
    /// The primary importer for EF4 implementation
    /// </summary>
    public class RepositoryImporter:IRepositoryImporter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <param name="newAttributeSets"></param>
        /// <param name="newEntities"></param>
        public void Import(int? zoneId, int? appId, IEnumerable<ContentType> newAttributeSets, IEnumerable<Entity> newEntities)
        {
            var import = new DbImport(zoneId, appId);
            import.ImportIntoDb(newAttributeSets, newEntities);
        }
    }
}

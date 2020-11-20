//using System.Collections.Generic;
//using System.Linq;
//using ToSic.Eav.Apps.ImportExport;
//using ToSic.Eav.Data;
//using ToSic.Eav.Interfaces;

//namespace ToSic.Eav.Repository.Efc.Implementations
//{
//    /// <summary>
//    /// The primary importer - important to keep importing decoupled from Eav.Core
//    /// </summary>
//    public class RepositoryImporter:IRepositoryImporter
//    {
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="zoneId"></param>
//        /// <param name="appId"></param>
//        /// <param name="newAttributeSets"></param>
//        /// <param name="newEntities"></param>
//        public void Import(int? zoneId, int appId, IEnumerable<ContentType> newAttributeSets, IEnumerable<Entity> newEntities)
//        {
//            var import = new Import(zoneId, appId);
//            import.ImportIntoDb(newAttributeSets.ToList(), newEntities.ToList());
//        }
//    }
//}

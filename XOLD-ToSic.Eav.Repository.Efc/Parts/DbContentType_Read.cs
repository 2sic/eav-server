using System.Linq;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbContentType: BllCommandBase
    {
        public DbContentType(DbDataController db) : base(db, "Db.Type") {}


        private ToSicEavAttributeSets GetTypeByStaticName(string staticName)
        {
            return DbContext.SqlDb.ToSicEavAttributeSets.FirstOrDefault(a =>
                a.AppId == DbContext.AppId && a.StaticName == staticName && a.ChangeLogDeleted == null
                );
        }
        
    }
}

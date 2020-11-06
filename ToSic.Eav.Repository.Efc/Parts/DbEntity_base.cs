using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity: BllCommandBase
    {
        public DbEntity(DbDataController db) : base(db, "Db.Enty")
        {
            Serializer = db.ServiceProvider.Build<JsonSerializer>();
        }
        private JsonSerializer Serializer { get; }
    }
}

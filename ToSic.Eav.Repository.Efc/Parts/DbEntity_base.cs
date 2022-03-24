using ToSic.Eav.ImportExport.Json;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity: BllCommandBase
    {
        public DbEntity(DbDataController db) : base(db, "Db.Enty")
        {
            Serializer = db.JsonSerializerGenerator.New;
        }
        private JsonSerializer Serializer { get; }
    }
}

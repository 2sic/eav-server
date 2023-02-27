using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Json;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity: BllCommandBase
    {

        public DbEntity(DbDataController db, MultiBuilder builder) : base(db, "Db.Enty")
        {
            _builder = builder;
            Serializer = db.JsonSerializerGenerator.New();
        }
        private JsonSerializer Serializer { get; }
        private readonly MultiBuilder _builder;
    }
}

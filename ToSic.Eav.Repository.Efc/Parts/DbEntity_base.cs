using ToSic.Eav.Data.Build;
using ToSic.Eav.ImportExport.Json;

namespace ToSic.Eav.Repository.Efc.Parts
{
    internal partial class DbEntity: DbPartBase
    {

        public DbEntity(DbDataController db, DataBuilder builder) : base(db, "Db.Enty")
        {
            _builder = builder;
            Serializer = db.JsonSerializerGenerator.New();
        }
        private JsonSerializer Serializer { get; }
        private readonly DataBuilder _builder;
    }
}

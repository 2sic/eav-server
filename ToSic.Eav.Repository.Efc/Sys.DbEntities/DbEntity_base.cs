using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbParts;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

internal partial class DbEntity(DbStorage.DbStorage db, DataAssembler dataAssembler) : DbPartBase(db, "Db.Enty")
{
    private JsonSerializer Serializer { get; } = db.JsonSerializerGenerator.New();
}
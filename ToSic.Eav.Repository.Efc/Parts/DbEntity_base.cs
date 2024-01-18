using ToSic.Eav.ImportExport.Json;

namespace ToSic.Eav.Repository.Efc.Parts;

internal partial class DbEntity(DbDataController db, DataBuilder builder) : DbPartBase(db, "Db.Enty")
{
    private JsonSerializer Serializer { get; } = db.JsonSerializerGenerator.New();
}
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbParts;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

internal partial class DbEntity(DbStorage.DbStorage db, DataAssembler dataAssembler) : DbPartBase(db, "Db.Enty")
{
    private IJsonDataWriter Serializer { get; } = db.JsonWriteGenerator.New();
}
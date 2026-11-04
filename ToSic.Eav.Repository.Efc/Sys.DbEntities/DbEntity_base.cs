using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

internal partial class DbEntity(DbStorage.DbStorage dbStore, DataAssembler dataAssembler) : HelperBase(dbStore.Log, "Db.Enty")
{
    private ILog? LogDetails => dbStore.LogDetails == null ? null : field ??= Log;
    private ILog? LogSummary => dbStore.LogSummary == null ? null : field ??= Log;

    private IJsonDataWriter Serializer { get; } = dbStore.JsonWriteGenerator.New();
}
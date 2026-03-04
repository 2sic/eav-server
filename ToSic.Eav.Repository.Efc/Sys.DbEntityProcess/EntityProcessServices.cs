using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbEntities;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal class EntityProcessServices(
    DbStorage.DbStorage dbStorage,
    DataAssembler dataAssembler,
    ICollection<IEntityPair<SaveOptions>> entityOptionPairs, ILog parentLog) : HelperBase(parentLog, "DB.PrepEy")
{
    #region Logging

    internal ILog? LogDetails => DbStorage.LogDetails == null ? null : field ??= Log;

    internal ILog? LogSummary => DbStorage.LogSummary == null ? null : field ??= Log;

    #endregion

    #region Helpers & Setup

    public DbStorage.DbStorage DbStorage = dbStorage;

    public DbEntity DbEntity = dbStorage.Entities;

    public DataAssembler DataAssembler = dataAssembler;

    public JsonSerializer Serializer { get; } = dbStorage.JsonSerializerGenerator.New();


    [field: AllowNull, MaybeNull]
    public EntityAnalyzeStructure StructureAnalyzer => field ??= new(DbStorage, LogDetails);

    [field: AllowNull, MaybeNull]
    public EntityAnalyzePublishing PublishingAnalyzer => field ??= new(DbStorage, DataAssembler, entityOptionPairs, LogDetails);

    public int TransactionId => _transactionId ??= DbStorage.Versioning.GetTransactionId();
    private int? _transactionId;

    #endregion
}

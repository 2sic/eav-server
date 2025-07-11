using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbEntities;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal class EntityProcessServices(DbStorage.DbStorage dbStorage, DataBuilder builder) : ServiceBase("DB.PrepEy")
{
    #region Logging

    internal ILog? LogDetails => DbStorage.LogDetails == null ? null : field ??= Log;

    internal ILog? LogSummary => DbStorage.LogSummary == null ? null : field ??= Log;

    #endregion

    #region Helpers & Setup

    public DbStorage.DbStorage DbStorage = dbStorage;

    public DbEntity DbEntity = dbStorage.Entities;

    public DataBuilder Builder = builder;

    public JsonSerializer Serializer { get; } = dbStorage.JsonSerializerGenerator.New();


    [field: AllowNull, MaybeNull]
    public EntityAnalyzeStructure StructureAnalyzer => field ??= new(DbStorage, LogDetails);

    [field: AllowNull, MaybeNull]
    public EntityAnalyzePublishing PublishingAnalyzer => field ??= new(DbStorage, Builder, LogDetails);

    public int TransactionId { get; set; }

    public void Start(ICollection<IEntityPair<SaveOptions>> entityOptionPairs)
    {
        // code as before, but we can probably remove later as object will not be reused....
        StructureAnalyzer.FlushTypeAttributesCache(); // for safety, in case previously new types were imported

        PublishingAnalyzer.Start(entityOptionPairs);

        TransactionId = DbStorage.Versioning.GetTransactionId();
    }

    #endregion
}

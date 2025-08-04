using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

/// <summary>
/// Experimental
///
/// Trying to extract logic steps for an entity to better reorganize sequencing.
/// </summary>
internal class SaveEntityProcess(DbStorage.DbStorage dbStorage, DataBuilder builder, ICollection<IEntityPair<SaveOptions>> entityOptionPairs, ILog parentLog) : HelperBase(parentLog, "DB.PrepEy")
{
    [field: AllowNull, MaybeNull]
    public EntityProcessServices Services => field ??= new(dbStorage, builder, entityOptionPairs, Log);

    public ICollection<EntityProcessData> ProcessBatch1(ICollection<EntityProcessData> data, bool logProcess)
        => ProcessBatch(data, GetStandardProcessBatch1(), logProcess);

    /// <summary>
    /// Batch 2 must run after all the entities have been created, to map the relationships
    /// </summary>
    /// <param name="data"></param>
    /// <param name="logProcess"></param>
    /// <returns></returns>
    public ICollection<EntityProcessData> ProcessBatch2(ICollection<EntityProcessData> data, bool logProcess)
        => ProcessBatch(data, GetStandardProcessBatch2(), logProcess);

    public ICollection<EntityProcessData> ProcessBatch(ICollection<EntityProcessData> data, List<IEntityProcess> batch, bool logProcess)
    {
        foreach (var process in batch)
        {
            data = process.Process(Services, data.NextStep(), logProcess);
            if (data.HasException(out var exception))
                throw exception;
        }

        return data;
    }

    private List<IEntityProcess> GetStandardProcessBatch1()
    {
        List<IEntityProcess> processors = 
        [
            new Process1Preflight(),
            new Process2PublishAndContentType(),

            new Process3New1LastChecks(),
            new Process3New2DbStoreNewHeaders(),
            new Process3New3DbStoreJson(),

            new Process3Upd1DbPreloadExisting(),
            new Process3Upd2PrepareUpdate(),
            new Process3Upd3ClearValues(),

            new Process4TableValues(),
            new Process4JsonValues(),

            //new Process5TableRelationships(),
            //new Process6Versioning(),
        ];

        foreach (var process in processors)
            process.LinkLog(Services.LogDetails);
        return processors;
    }
    private List<IEntityProcess> GetStandardProcessBatch2()
    {
        List<IEntityProcess> processors = 
        [
            new Process5TableRelationships(),

            new Process6Versioning(),
        ];

        foreach (var process in processors)
            process.LinkLog(Services.LogDetails);
        return processors;
    }
}
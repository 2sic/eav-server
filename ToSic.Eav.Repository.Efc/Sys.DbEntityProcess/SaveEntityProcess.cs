using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

/// <summary>
/// Experimental
///
/// Trying to extract logic steps for an entity to better reorganize sequencing.
/// </summary>
internal class SaveEntityProcess(DbStorage.DbStorage dbStorage, DataBuilder builder) : ServiceBase("DB.PrepEy")
{

    #region Helpers & Setup

    public EntityProcessServices Services { get; set; } = null!;

    public void Start(ICollection<IEntityPair<SaveOptions>> entityOptionPairs)
    {
        Services = new(dbStorage, builder);
        Services.Start(entityOptionPairs);
    }

    #endregion

    public EntityProcessData Process(IEntityPair<SaveOptions> entityOptionPair, bool logDetails)
    {
        // 1. Prepare data
        var data = new EntityProcessData
        {
            NewEntity = entityOptionPair.Entity,
            Options = entityOptionPair.Partner,
            LogDetails = logDetails,
            Progress = 0,
        };

        foreach (var process in GetStandardProcess())
        {
            data = process.Process(Services, data.NextStep());
            if (data.Exception != null)
                throw data.Exception;
        }

        return data;
    }

    private List<IEntityProcess> GetStandardProcess() =>
    [
        new Process1Preflight(),
        new Process2PublishAndContentType(),
        
        new Process3New1LastChecks(),
        new Process3New2DbStoreHeader(),
        new Process3New3DbStoreJson(),

        new Process3Upd1DbPreload(),
        new Process3Upd2PrepareUpdate(),
        new Process3Upd3ClearValues(),

        new Process4TableValues(),
        new Process4JsonValues(),

        new Process5TableRelationships(),

        new Process6Versioning(),
    ];

}
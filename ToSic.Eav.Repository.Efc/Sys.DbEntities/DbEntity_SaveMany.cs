using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

partial class DbEntity
{
    /// <summary>
    /// Save a list of entities in one large go
    /// </summary>
    /// <param name="entityOptionPairs"></param>
    /// <returns></returns>
    internal List<EntityIdentity> SaveEntities(ICollection<IEntityPair<SaveOptions>> entityOptionPairs)
    {
        // wrong toggle, but it's something people don't have ATM
        //var useNewSave = DbContext.Features.IsEnabled(BuiltInFeatures.DataImportParallel);
        var useParallel = DbStore.Features.IsEnabled(BuiltInFeatures.DatabaseWriteOptimized);

        var l = LogSummary.Fn<List<EntityIdentity>>($"count:{entityOptionPairs.Count}; Optimized: {useParallel}");

        if (entityOptionPairs.Count == 0)
            return l.Return([], "Entities to save are empty, skip");

        var saveProcess = new SaveEntityProcess(DbStore, dataAssembler, entityOptionPairs, Log);

        var ids = useParallel
            ? SaveEntitiesParallel(saveProcess, entityOptionPairs)
            : SaveEntitiesSerialNew(saveProcess, entityOptionPairs);

        return l.Return(ids, $"id count:{ids.Count}");
    }


    /// <summary>
    /// Newer mechanism using the SaveEntityProcess only.
    /// </summary>
    /// <param name="saveProcess"></param>
    /// <param name="entityOptionPairs"></param>
    /// <returns></returns>
    internal List<EntityIdentity> SaveEntitiesParallel(SaveEntityProcess saveProcess, ICollection<IEntityPair<SaveOptions>> entityOptionPairs)
    {
        var l = LogSummary.Fn<List<EntityIdentity>>();

        var data = entityOptionPairs
            .Select((eop, i) => EntityProcessData.CreateInstance(eop, i < DbConstant.MaxToLogDetails))
            .ToListOpt();

        ICollection<EntityProcessData> result = null!;

        DbStore.DoInTransaction(() =>
        {
            result = saveProcess.ProcessBatch1(data, true);
            result = saveProcess.ProcessBatch2(result, true);
        });

        var ids = result
            .Select(d => d.Ids)
            .ToList();

        return l.Return(ids);
    }


    /// <summary>
    /// Newer mechanism using the SaveEntityProcess only.
    /// </summary>
    /// <param name="saveProcess"></param>
    /// <param name="entityOptionPairs"></param>
    /// <returns></returns>
    internal List<EntityIdentity> SaveEntitiesSerialNew(SaveEntityProcess saveProcess, ICollection<IEntityPair<SaveOptions>> entityOptionPairs)
    {
        var l = LogSummary.Fn<List<EntityIdentity>>();
        var ids = new List<EntityIdentity>();
        var idx = 0;

        DbStore.DoInTransaction(
            () => 
            {
                List<EntityProcessData> batch1 = [];

                foreach (var pair in entityOptionPairs)
                {
                    // Logging, but only the first 250 or so entries...
                    idx++;
                    var logDetails = idx < DbConstant.MaxToLogDetails;
                    if (idx == DbConstant.MaxToLogDetails)
                        l.A($"Hit #{idx}, will stop logging details");

                    // Actually do the work...
                    ICollection<EntityProcessData> data = [EntityProcessData.CreateInstance(pair, logDetails)];
                    var result = saveProcess.ProcessBatch1(data, false);
                    batch1.AddRange(result);
                }

                foreach (var pair in batch1)
                {
                    var result = saveProcess.ProcessBatch2([pair], false);
                    ids.Add(result.First().Ids);
                }
            }
        );
        return l.Return(ids);
    }

    /// <summary>
    /// Old mechanism using the one-by-one save and queuing.
    /// </summary>
    /// <param name="saveProcess"></param>
    /// <param name="entityOptionPairs"></param>
    /// <returns></returns>
    internal List<EntityIdentity> SaveEntitiesSerialOld(SaveEntityProcess saveProcess, ICollection<IEntityPair<SaveOptions>> entityOptionPairs)
    {
        var l = LogSummary.Fn<List<EntityIdentity>>();
        var ids = new List<EntityIdentity>();
        var idx = 0;

        DbStore.DoInTransaction(
            () =>
            {
                foreach (var pair in entityOptionPairs)
                {
                    // Logging, but only the first 250 or so entries...
                    idx++;
                    var logDetails = idx < DbConstant.MaxToLogDetails;
                    if (idx == DbConstant.MaxToLogDetails)
                        l.A($"Hit #{idx}, will stop logging details");

                    DbStore.DoAndSaveWithoutChangeDetection(
                        () =>
                        {
                            var saved = SaveEntity(pair, saveProcess, logDetails);
                            ids.Add(saved);
                        },
                        "SaveMany-old"
                    );
                }
            }
        );
        return l.Return(ids);
    }
}
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
        var useNewSave = DbContext.Features.IsEnabled(BuiltInFeatures.DataImportParallel);
        var useParallel = DbContext.Features.IsEnabled(BuiltInFeatures.DataImportParallel);

        var l = LogSummary.Fn<List<EntityIdentity>>($"count:{entityOptionPairs.Count}; Optimized: {useNewSave}");

        if (entityOptionPairs.Count == 0)
            return l.Return([], "Entities to save are null, skip");

        var ids = new List<EntityIdentity>();
        var idx = 0;

        var saveProcess = new SaveEntityProcess(DbContext, builder, entityOptionPairs);

        DbContext.DoInTransaction(
            () => DbContext.Versioning.DoAndSaveHistoryQueue(
                () => DbContext.Relationships.DoWhileQueueingRelationships(
                    () => DoWhileQueueingAttributes(
                        () =>
                        {
                            foreach (var pair in entityOptionPairs)
                            {
                                // Logging, but only the first 250 or so entries...
                                idx++;
                                var logDetails = idx < DbConstant.MaxToLogDetails;
                                if (idx == DbConstant.MaxToLogDetails)
                                    l.A($"Hit #{idx}, will stop logging details");

                                // Actually do the work...
                                if (useNewSave)
                                {
                                    DbContext.DoAndSaveWithoutChangeDetection(
                                        () =>
                                        {
                                            ICollection<EntityProcessData> data = [EntityProcessData.CreateInstance(pair, logDetails)];
                                            var result = saveProcess.Process(data);
                                            ids.Add(result.First().Ids);
                                        },
                                        "SaveMany-new"
                                    );

                                }
                                else
                                {
                                    DbContext.DoAndSaveWithoutChangeDetection(
                                        () =>
                                        {
                                            var saved = SaveEntity(pair, saveProcess, logDetails);
                                            ids.Add(saved);
                                        },
                                        "SaveMany-old"
                                    );
                                }
                            }
                        }
                    )
                )
            )
        );
        return l.Return(ids, $"id count:{ids.Count}");
    }

    //internal List<int> SaveEntitiesOld(SaveEntityProcess saveProcess, ICollection<IEntityPair<SaveOptions>> entityOptionPairs)
    //{
    //    DbContext.DoInTransaction(
    //        () => DbContext.Versioning.DoAndSaveHistoryQueue(
    //            () => DbContext.Relationships.DoWhileQueueingRelationships(
    //                () => DoWhileQueueingAttributes(
    //                    () =>
    //                    {
    //                        foreach (var pair in entityOptionPairs)
    //                        {
    //                            // Logging, but only the first 250 or so entries...
    //                            idx++;
    //                            var logDetails = idx < DbConstant.MaxToLogDetails;
    //                            if (idx == DbConstant.MaxToLogDetails)
    //                                l.A($"Hit #{idx}, will stop logging details");

    //                            // Actually do the work...
    //                            if (useNewSave)
    //                            {
    //                                DbContext.DoAndSaveWithoutChangeDetection(
    //                                    () =>
    //                                    {
    //                                        ICollection<EntityProcessData> data = [EntityProcessData.CreateInstance(pair, logDetails)];
    //                                        var result = saveProcess.Process(data);
    //                                        ids.Add(result.First().FinalId);
    //                                        TempLastSaveGuid = result.First().FinalGuid;
    //                                    },
    //                                    "SaveMany-new"
    //                                );

    //                            }
    //                            else
    //                            {
    //                                DbContext.DoAndSaveWithoutChangeDetection(
    //                                    () => ids.Add(SaveEntity(pair, saveProcess, logDetails)),
    //                                    "SaveMany-old"
    //                                );
    //                            }
    //                        }
    //                    }
    //                )
    //            )
    //        )
    //    );
    //}
}
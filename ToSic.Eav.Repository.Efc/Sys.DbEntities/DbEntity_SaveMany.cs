using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

partial class DbEntity
{
    internal SaveEntityProcess Preprocessor { get; set; } = null!;

    /// <summary>
    /// Save a list of entities in one large go
    /// </summary>
    /// <param name="entityOptionPairs"></param>
    /// <returns></returns>
    internal List<int> SaveEntities(ICollection<IEntityPair<SaveOptions>> entityOptionPairs)
    {
        // wrong toggle, but it's something people don't have ATM
        var useNewSave = DbContext.Features.IsEnabled(BuiltInFeatures.DataImportParallel);
        var useParallel = DbContext.Features.IsEnabled(BuiltInFeatures.DataImportParallel);

        var l = LogSummary.Fn<List<int>>($"count:{entityOptionPairs.Count}; Optimized: {useNewSave}");

        if (entityOptionPairs.Count == 0)
            return l.Return([], "Entities to save are null, skip");

        var ids = new List<int>();
        var idx = 0;

        Preprocessor = new(DbContext, builder);
        Preprocessor.Start(entityOptionPairs);

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
                                    Preprocessor.Process(pair, logDetails);
                                }
                                else
                                {
                                    DbContext.DoAndSaveWithoutChangeDetection(
                                        () => ids.Add(SaveEntity(pair, logDetails)),
                                        "SaveMany"
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

}
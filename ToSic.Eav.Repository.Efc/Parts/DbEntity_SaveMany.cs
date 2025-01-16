namespace ToSic.Eav.Repository.Efc.Parts;

partial class DbEntity
{
    /// <summary>
    /// Save a list of entities in one large go
    /// </summary>
    /// <param name="entityOptionPairs"></param>
    /// <param name="saveOptions"></param>
    /// <returns></returns>
    internal List<int> SaveEntity(List<IEntityPair<SaveOptions>> entityOptionPairs/*, SaveOptions saveOptions*/)
    {
        var l = Log.Fn<List<int>>($"count:{entityOptionPairs?.Count}");

        if (entityOptionPairs == null || entityOptionPairs.Count == 0)
            return l.Return([], "Entities to save are null, skip");

        var ids = new List<int>();
        var idx = 0;
        FlushTypeAttributesCache(); // for safety, in case previously new types were imported
        _entityDraftMapCache = DbContext.Publishing
            .GetDraftBranchMap(entityOptionPairs.Select(e => e.Entity.EntityId).ToList());

        DbContext.DoInTransaction(
            () => DbContext.Versioning.DoAndSaveHistoryQueue(
                () => DbContext.Relationships.DoWhileQueueingRelationships(
                    () => DoWhileQueueingAttributes(
                        () =>
                        {
                            foreach (var pair in entityOptionPairs)
                            {
                                idx++;
                                var logDetails = idx < MaxToLogDetails;
                                if (idx == MaxToLogDetails)
                                    l.A($"Hit #{idx}, will stop logging details");
                                DbContext.DoAndSaveWithoutChangeDetection(
                                    () => ids.Add(SaveEntity(pair, logDetails)),
                                    "SaveMany"
                                );
                            }
                        }
                    )
                )
            )
        );
        return l.Return(ids, $"id count:{ids.Count}");
    }

}
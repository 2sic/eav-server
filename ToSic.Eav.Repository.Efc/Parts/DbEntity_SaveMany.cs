namespace ToSic.Eav.Repository.Efc.Parts;

partial class DbEntity
{
    /// <summary>
    /// Save a list of entities in one large go
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="saveOptions"></param>
    /// <returns></returns>
    internal List<int> SaveEntity(List<IEntity> entities, SaveOptions saveOptions)
    {
        var l = Log.Fn<List<int>>($"count:{entities?.Count}");

        if (entities == null || entities.Count == 0)
            return l.Return([], "Entities to save are null, skip");

        var ids = new List<int>();
        var idx = 0;
        FlushTypeAttributesCache(); // for safety, in case previously new types were imported
        _entityDraftMapCache = DbContext.Publishing
            .GetDraftBranchMap(entities.Select(e => e.EntityId).ToList());
        DbContext.DoInTransaction(
            () => DbContext.Versioning.DoAndSaveHistoryQueue(
                () => DbContext.Relationships.DoWhileQueueingRelationships(
                    () => DoWhileQueueingAttributes(
                        () =>
                        {
                            foreach (var e in entities)
                            {
                                idx++;
                                var logDetails = idx < MaxToLogDetails;
                                if (idx == MaxToLogDetails)
                                    l.A($"Hit #{idx}, will stop logging details");
                                DbContext.DoAndSaveWithoutChangeDetection(
                                    () => ids.Add(SaveEntity(e, saveOptions, logDetails)),
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
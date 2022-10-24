using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity
    {
        /// <summary>
        /// Save a list of entities in one large go
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="saveOptions"></param>
        /// <returns></returns>
        internal List<int> SaveEntity(List<IEntity> entities, SaveOptions saveOptions)
        {
            var wrapLog = Log.Fn<List<int>>($"count:{entities?.Count}");
            var ids = new List<int>();

            if (entities == null || entities.Count == 0)
                Log.A("Entities to save are null, skip");
            else
            {
                var idx = 0;
                FlushTypeAttributesCache(); // for safety, in case previously new types were imported
                EntityDraftMapCache = DbContext.Publishing.GetDraftBranchMap(entities.Select(e => e.EntityId).ToList());
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
                                        if (idx == MaxToLogDetails) Log.A($"Hit #{idx}, will stop logging details");
                                        DbContext.DoAndSaveWithoutChangeDetection(() => ids.Add(SaveEntity(e, saveOptions, logDetails)),
                                            "SaveMany");
                                    }
                                }))));
            }
            return wrapLog.Return(ids, $"id count:{ids.Count}");
        }

    }
}

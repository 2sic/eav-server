using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Lib.Logging;
using ToSic.Eav.WebApi.Formats;
using ToSic.Lib.Services;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi.SaveHelpers
{
    public class SaveEntities: HelperBase
    {
        public SaveEntities(ILog parentLog) : base(parentLog, "Eav.SavHlp") {}


        public void UpdateGuidAndPublishedAndSaveMany(AppManager appMan, List<BundleWithHeader<IEntity>> itemsToImport,
            bool enforceDraft
        ) => Log.Do(l =>
        {
            foreach (var bundle in itemsToImport)
            {
                var curEntity = (Entity)bundle.Entity;
                curEntity.SetGuid(bundle.Header.Guid);
                if (enforceDraft)
                    EnforceDraft(curEntity);
            }

            var entitiesToImport = itemsToImport.Select(e => e.Entity).ToList();

            l.A("will save " + entitiesToImport.Count + " items");
            appMan.Entities.Save(entitiesToImport);
        });

        private void EnforceDraft(Entity currEntity) => Log.Do($"will set published/isbranch on {currEntity.EntityGuid}", () =>
        {
            currEntity.IsPublished = false;
            currEntity.PlaceDraftInBranch = true;
        });

        /// <summary>
        /// Generate pairs of guid/id of the newly added items
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, int> GenerateIdList(EntityRuntime appEntities, IEnumerable<BundleWithHeader> items)
        {
            var wrapLog = Log.Fn<Dictionary<Guid, int>>();
            var idList = items.Select(e =>
                {
                    var foundEntity = appEntities.Get(e.Header.Guid);
                    var state = foundEntity == null ? "not found" : foundEntity.IsPublished ? "published" : "draft";
                    var draft = foundEntity?.GetDraft();
                    Log.A(
                        $"draft check: entity {e.Header.Guid} ({state}) - additional draft: {draft != null} - will return the draft");
                    return
                        draft ??
                        foundEntity; // return the draft (that would be the latest), or the found, or null if not found
                })
                .Where(e => e != null)
                .ToDictionary(f => f.EntityGuid, f => f.EntityId);
            return wrapLog.Return(idList);
        }

    }
}

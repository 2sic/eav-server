using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging;
using ToSic.Eav.WebApi.Formats;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi.SaveHelpers
{
    public class SaveEntities: HasLog
    {
        public SaveEntities(ILog parentLog) : base("Eav.SavHlp", parentLog) {}


        public void UpdateGuidAndPublishedAndSaveMany(AppManager appMan, List<BundleWithHeader<IEntity>> itemsToImport, bool enforceDraft)
        {
            var wrapLog = Log.Call("");
            foreach (var bundle in itemsToImport)
            {
                var currEntity = (Entity)bundle.Entity;
                currEntity.SetGuid(bundle.Header.Guid);
                if (enforceDraft)
                    EnforceDraft(currEntity);
            }

            var entitiesToImport = itemsToImport.Select(e => e.Entity).ToList();

            Log.Add("will save " + entitiesToImport.Count + " items");
            appMan.Entities.Save(entitiesToImport);
            wrapLog(null);
        }

        private void EnforceDraft(Entity currEntity)
        {
            var wrapLog = Log.Call($"will set published/isbranch on {currEntity.EntityGuid}");
            currEntity.IsPublished = false;
            currEntity.PlaceDraftInBranch = true;
            wrapLog(null);
        }

        /// <summary>
        /// Generate pairs of guid/id of the newly added items
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, int> GenerateIdList(EntityRuntime appEntities, IEnumerable<BundleWithHeader> items)
        {
            var wrapLog = Log.Call();
            var idList = items.Select(e =>
                {
                    var foundEntity = appEntities.Get(e.Header.Guid);
                    var state = foundEntity == null ? "not found" : foundEntity.IsPublished ? "published" : "draft";
                    var draft = foundEntity?.GetDraft();
                    Log.Add(
                        $"draft check: entity {e.Header.Guid} ({state}) - additional draft: {draft != null} - will return the draft");
                    return
                        draft ??
                        foundEntity; // return the draft (that would be the latest), or the found, or null if not found
                })
                .Where(e => e != null)
                .ToDictionary(f => f.EntityGuid, f => f.EntityId);
            wrapLog(null);
            return idList;
        }

    }
}

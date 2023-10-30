using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.AppSys;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data.Build;
using ToSic.Lib.Logging;
using ToSic.Eav.WebApi.Formats;
using ToSic.Lib.Services;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi.SaveHelpers
{
    public class SaveEntities: ServiceBase
    {
        private readonly EntityBuilder _entityBuilder;
        public SaveEntities(EntityBuilder entityBuilder) : base("Eav.SavHlp")
        {
            ConnectServices(
                _entityBuilder = entityBuilder
            );
        }


        public void UpdateGuidAndPublishedAndSaveMany(AppManager initializedAppMan, List<BundleWithHeader<IEntity>> itemsToImport,
            bool enforceDraft
        ) => Log.Do(l =>
        {
            //foreach (var bundle in itemsToImport)
            //{
            //    var curEntity = (Entity)bundle.Entity;
            //    curEntity.SetGuid(bundle.Header.Guid);
            //    if (enforceDraft)
            //        EnforceDraft(curEntity);
            //}

            var entitiesToImport = itemsToImport
                // TODO: NOTE: here a clone should work
                .Select(bundle => _entityBuilder.CreateFrom(bundle.Entity,
                    guid: bundle.Header.Guid,
                    isPublished: enforceDraft ? (bool?)false : null,
                    placeDraftInBranch: enforceDraft ? (bool?)true : null) as IEntity
                )
                //.Select(bundle => _entityBuilder.ResetIdentifiers(bundle.Entity,
                //    newGuid: bundle.Header.Guid,
                //    isPublished: enforceDraft ? (bool?)false : null,
                //    placeDraftInBranch: enforceDraft ? (bool?)true : null)
                //)
                .ToList();

            l.A($"will save {entitiesToImport.Count} items");
            initializedAppMan.Entities.Save(entitiesToImport);
        });

        /// <summary>
        /// Generate pairs of guid/id of the newly added items
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, int> GenerateIdList(IAppWorkCtx appCtx, AppEntityRead appEntities, IEnumerable<BundleWithHeader> items)
        {
            var l = Log.Fn<Dictionary<Guid, int>>();
            
            var idList = items.Select(e =>
                {
                    var foundEntity = appEntities.Get(appCtx, e.Header.Guid);
                    var state = foundEntity == null ? "not found" : foundEntity.IsPublished ? "published" : "draft";
                    var draft = foundEntity  == null ? null : appCtx.AppState.GetDraft(foundEntity);
                    l.A($"draft check: entity {e.Header.Guid} ({state}) - additional draft: {draft != null} - will return the draft");
                    return draft ?? foundEntity; // return the draft (that would be the latest), or the found, or null if not found
                })
                .Where(e => e != null)
                .ToDictionary(f => f.EntityGuid, f => f.EntityId);
            return l.ReturnAsOk(idList);
        }

    }
}

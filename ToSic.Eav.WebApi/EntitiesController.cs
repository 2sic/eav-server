using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ToSic.Eav.Apps;
using ToSic.Eav.WebApi.Formats;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.WebApi
{
    /// <inheritdoc />
    /// <summary>
    /// Web API Controller for various actions
    /// </summary>
    public class EntitiesController : Eav3WebApiBase
    {
        public EntitiesController(int appId) : base(appId) { }
        public EntitiesController(Log parentLog) : base(parentLog) { }

        public Dictionary<Guid, int> SaveMany([FromUri] int appId, [FromBody] List<BundleWithHeader<EntityWithLanguages>> items,
            [FromUri] bool partOfPage = false, bool draftOnly = false)
        {
            //todo: remove this once we're sure we're not using the global appid for anything
            SetAppId(appId);
            var appMan = new AppManager(appId, Log);
            Log.Add($"SaveMany(appId:{appId}, items:{items.Count}, partOfPage (not used here, must be done in dnn):{partOfPage}, draftOnly:{draftOnly})");

            #region 2018-09-07 this area should probably be disabled, but I'm not sure if it's still needed - better leave it in, till this is deprecated
            // #1 set guid
            // must move guid from header to entity, because we only transfer it on the header (so no duplicates)
            foreach (var i in items)
                //i.EntityGuid = i.Header.Guid; //
                i.Entity.Guid = i.Header.Guid;

            // #2 ensure draft/branch state
            if (draftOnly)
                foreach (var i in items)
                {
                    Log.Add($"draft only, will set published/isbranch on {i.Header.Guid}");
                    i.Entity.IsPublished = false;
                    i.Entity.IsBranch = true;
                }
            #endregion

            var oldSave = new SaveHelpers.OldSave(Log);
            var entitySetToImport = items
                .Where(entity => entity.Header.Group == null || !entity.Header.Group.SlotIsEmpty)
                .Select(e => new BundleWithHeader<IEntity>
                {
                    Header = e.Header,
                    Entity = oldSave.CreateEntityFromTransferObject(appMan, e)
                })
                .ToList();

            var save= new SaveHelpers.SaveEntities(Log);
            save.UpdateGuidAndPublishedAndSaveMany(appMan, entitySetToImport, draftOnly);
            return save.GenerateIdList(appMan.Read.Entities, items);
        }


    }
}
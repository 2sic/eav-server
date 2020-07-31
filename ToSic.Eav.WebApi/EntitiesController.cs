// #2134
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using ToSic.Eav.Apps;
//using ToSic.Eav.WebApi.Formats;
//using ToSic.Eav.Logging;
//using IEntity = ToSic.Eav.Data.IEntity;

//namespace ToSic.Eav.WebApi
//{
//    /// <inheritdoc />
//    /// <summary>
//    /// Web API Controller for various actions
//    /// </summary>
//    public class EntitiesController : HasLog
//    {
//        public EntitiesController(ILog parentLog) : base("Api.EntCtl", parentLog) { }

//        public Dictionary<Guid, int> SaveMany(int appId, 
//            List<BundleWithHeader<EntityWithLanguages>> items,
//            bool partOfPage, 
//            bool draftOnly)
//        {
//            //todo: remove this once we're sure we're not using the global appid for anything
//            //SetAppId(appId);
//            var appMan = new AppManager(appId, Log);
//            var wrapLog = Log.Call($"appId:{appId}, items:{items.Count}, partOfPage (not used here, must be done in dnn):{partOfPage}, draftOnly:{draftOnly}");

//            #region 2018-09-07 this area should probably be disabled, but I'm not sure if it's still needed - better leave it in, till this is deprecated
//            // #1 set guid
//            // must move guid from header to entity, because we only transfer it on the header (so no duplicates)
//            foreach (var i in items)
//                //i.EntityGuid = i.Header.Guid; //
//                i.Entity.Guid = i.Header.Guid;

//            // #2 ensure draft/branch state
//            // 2018-09-26 2dm - this is moved to a later point in the code
//            //foreach (var i in items)
//            //{
//            //    if (draftOnly)
//            //    {
//            //        Log.Add($"forceDraft:{draftOnly}, will change from isPublished:{i.Entity.IsPublished} isBranch:{i.Entity.IsBranch}");
//            //        i.Entity.IsPublished = false;
//            //        i.Entity.IsBranch = true;
//            //    }
//            //    Log.Add($"after forceDraft check item:{i.Header.Guid} - isPublished:{i.Entity.IsPublished} isBranch:{i.Entity.IsBranch}");
//            //}
                
//            #endregion

//            var oldSave = new SaveHelpers.OldSave(Log);
//            var entitySetToImport = items
//                .Where(entity => entity.Header.Group == null || !entity.Header.Group.SlotIsEmpty)
//                .Select(e => new BundleWithHeader<IEntity>
//                {
//                    Header = e.Header,
//                    Entity = oldSave.CreateEntityFromTransferObject(appMan, e)
//                })
//                .ToList();

//            var save= new SaveHelpers.SaveEntities(Log);
//            save.UpdateGuidAndPublishedAndSaveMany(appMan, entitySetToImport, draftOnly);
//            wrapLog("ok");
//            return save.GenerateIdList(appMan.Read.Entities, items);
//        }


//    }
//}
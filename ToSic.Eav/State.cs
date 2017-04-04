using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ToSic.Eav.BLL;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav
{
    static class State
    {

        #region Metadata & Assignment-Types
        /// <summary>
        /// Retrieve the Assignment-Type-ID which is used to determine which type of object
        /// an entity is assigned to (because just the object ID would be ambiguous)
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static int GetAssignmentTypeId(string typeName)
            => DataSource.GetCache(null).GetAssignmentObjectTypeId(typeName);
        #endregion

        #region purge cache stuff
        public static void Purge(int zoneId, int appId, bool global = false)
        {
            if (global)
                DataSource.GetCache(null).PurgeGlobalCache();
            else
                DataSource.GetCache(null).PurgeCache(zoneId, appId);
        }

        public static void DoAndPurge(int zoneId, int appId, Action action, bool global=false)
        {
            action.Invoke();
            Purge(zoneId, appId, global);
        }
        #endregion

        #region Zone commands

        public static int ZoneCreate(string name)
        {
            var zoneId = EavDataController.Instance()
                .Zone.AddZone(name).Item1.ZoneID;
            Purge(zoneId, GetDefaultAppId(zoneId), true);
            return zoneId;
        }
        #endregion

        #region Zone Language / Culture / Dimension commands
        public static List<Tuple<bool, string>> ZoneLanguages(int zoneId)
        {
            return EavDataController.Instance(zoneId).Dimensions.GetLanguages()
                .Select(d => new Tuple<bool, string>(d.Active, d.ExternalKey))
                .ToList();
        }

        public static void ZoneLanguageAddOrUpdate(int zoneId, string cultureCode, string cultureText, bool active)
            => EavDataController.Instance(zoneId).Dimensions.AddOrUpdateLanguage(cultureCode, cultureText, active);

        #endregion

        #region App Commands


        public static int GetDefaultAppId(int zoneId)
            => ((BaseCache)DataSource.GetCache(zoneId)).ZoneApps[zoneId].DefaultAppId;

        public static void AppDelete(int zoneId, int appId)
            => DoAndPurge(zoneId, appId, () => EavDataController.Instance(zoneId, appId).App.DeleteApp(appId), true);

        public static int AppCreate(int zoneId)
        {
            var eavContext = EavDataController.Instance(zoneId);
            var app = eavContext.App.AddApp(null, Guid.NewGuid().ToString());

            Purge(zoneId, app.AppID, true);
            return app.AppID;
        }

        public static Dictionary<int, string> GetAppList(int zoneId)
            => ((BaseCache)DataSource.GetCache(zoneId)).ZoneApps[zoneId].Apps;

        public static string GetAppName(int zoneId, int appId)
            => ((BaseCache) DataSource.GetCache(zoneId)).ZoneApps[zoneId].Apps[appId];

        #endregion

        #region ContentType Commands

        public static IEnumerable<IContentType> ContentTypes(int zoneId, int appId, string scope = null, bool includeAttributeTypes = false)
        {
            var contentTypes = ((BaseCache)DataSource.GetCache(zoneId, appId)).GetContentTypes();
            var set = contentTypes.Select(c => c.Value)
                .Where(c => includeAttributeTypes || !c.Name.StartsWith("@"));
            if(scope != null)
                set = set.Where(p => p.Scope == scope);
            return set.OrderBy(c => c.Name); 
        }
        #endregion

        #region Data-level entity actions

        public static void EntityUpdate(int zoneId, int appId, int id, Dictionary<string, object> values)
            => EavDataController.Instance(zoneId, appId).Entities.UpdateEntity(id, values);

        public static Tuple<int, Guid> EntityCreate(int zoneId, int appId, string typeName, Dictionary<string, object> values, Guid? entityGuid = null)
        {
            var contentType = DataSource.GetCache(zoneId, appId).GetContentType(typeName);
            var ent = EavDataController.Instance(zoneId, appId).Entities.AddEntity(contentType.AttributeSetId, values, null, null, entityGuid: entityGuid);
            return new Tuple<int, Guid>(ent.EntityID, ent.EntityGUID);
        }

        public static bool EntityDelete(int zoneId, int appId, int id)
        {
            var eavContext = EavDataController.Instance(zoneId, appId);
            var canDelete = eavContext.Entities.CanDeleteEntity(id);
            if (!canDelete.Item1)
                throw new Exception(canDelete.Item2);
            return eavContext.Entities.DeleteEntity(id);
        }

        public static int EntityGetOrCreate(int zoneId, int appId, Guid? newGuid, string contentTypeName, Dictionary<string, object> values)
        {
            var ctl = EavDataController.Instance(zoneId, appId);
            if (newGuid.HasValue && ctl.Entities.EntityExists(newGuid.Value)) // eavDc.Entities.EntityExists(newGuid.Value))
            {
                // check if it's deleted - if yes, resurrect
                var existingEnt = ctl.Entities.GetEntitiesByGuid(newGuid.Value).First();
                if (existingEnt.ChangeLogDeleted != null)
                    existingEnt.ChangeLogDeleted = null;

                return existingEnt.EntityID;
            }

            return EntityCreate(zoneId, appId, contentTypeName, values, entityGuid: newGuid).Item1;
        }

        //public static bool EntityExists(int zoneId, int appId, Guid guid) 
        //    => EavDataController.Instance(zoneId, appId).Entities.EntityExists(guid);

        //public static int EntityGetOrResurrect(int zoneId, int appId, Guid guid)
        //{
        //    var existingEnt = EavDataController.Instance(zoneId, appId).Entities.GetEntitiesByGuid(guid).First();
        //    if (existingEnt.ChangeLogDeleted != null)
        //        existingEnt.ChangeLogDeleted = null;
        //    return existingEnt.EntityID;
        //}

        #endregion


        public static void MetadataEnsureTypeAndSingleEntity(int zoneId, int appId, string scope, string setName,
            string label, int appAssignment, OrderedDictionary values)
        {
            var eavContext = EavDataController.Instance(zoneId, appId);

            var contentType = !eavContext.AttribSet.AttributeSetExists(setName, eavContext.AppId)
                ? eavContext.AttribSet.AddContentTypeAndSave(setName, label, setName, scope)
                : eavContext.AttribSet.GetAttributeSet(setName);

            if (values == null)
                values = new OrderedDictionary();

            eavContext.Entities.AddEntity(contentType, values, null, eavContext.AppId, appAssignment);

            Purge(eavContext.ZoneId, eavContext.ZoneId); 
       }




    }
}

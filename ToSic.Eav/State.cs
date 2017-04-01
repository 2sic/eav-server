using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using ToSic.Eav.BLL;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav
{
    static class State
    {

        public static int GetDefaultAppId(int zoneId)
            => ((BaseCache) DataSource.GetCache(zoneId)).ZoneApps[zoneId].DefaultAppId;

        /// <summary>
        /// Retrieve the Assignment-Type-ID which is used to determine which type of object
        /// an entity is assigned to (because just the object ID would be ambiguous)
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static int GetAssignmentTypeId(string typeName)
            => DataSource.GetCache(null).GetAssignmentObjectTypeId(typeName);

        public static void Purge(int zoneId, int appId)
            => DataSource.GetCache(null).PurgeCache(zoneId, appId);

        public static void DoAndPurge(int zoneId, int appId, Action action)
        {
            action.Invoke();
            Purge(zoneId, appId);
        }

        public static void AppDelete(int zoneId, int appId)
            => DoAndPurge(zoneId, appId, () => EavDataController.Instance(zoneId, appId).App.DeleteApp(appId));

        public static int AppCreate(int zoneId)
        {
            var eavContext = EavDataController.Instance(zoneId);
            var app = eavContext.App.AddApp(null, Guid.NewGuid().ToString());

            Purge(zoneId, app.AppID);
            return app.AppID;
        }

        public static Dictionary<int, string> GetAppList(int zoneId)
            => ((BaseCache)DataSource.GetCache(zoneId)).ZoneApps[zoneId].Apps;

        public static string GetAppName(int zoneId, int appId)
            => ((BaseCache) DataSource.GetCache(zoneId)).ZoneApps[zoneId].Apps[appId];


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

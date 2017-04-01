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
    }
}

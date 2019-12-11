using ToSic.Eav.Apps.Caching;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps
{
    public class SystemRuntime
    {
        /// <summary>
        /// Quickly lookup the zone id of an app
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static int ZoneIdOfApp(int appId) => Factory.Resolve<IAppsCache>().GetIdentity(appId: appId).ZoneId;

        /// <summary>
        /// Retrieve the Assignment-Type-ID which is used to determine which type of object
        /// an entity is assigned to (because just the object ID would be ambiguous)
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static int MetadataType(string typeName) => Factory.Resolve<ITargetTypes>().GetId(typeName);

        public static string MetadataType(int typeNumber) => Factory.Resolve<ITargetTypes>().GetName(typeNumber);

    }
}

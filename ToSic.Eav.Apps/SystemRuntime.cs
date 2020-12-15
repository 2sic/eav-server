namespace ToSic.Eav.Apps
{
    public class SystemRuntime
    {
        /// <summary>
        /// Quickly lookup the zone id of an app
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static int ZoneIdOfApp(int appId) => State.Identity(null, appId).ZoneId;
    }
}

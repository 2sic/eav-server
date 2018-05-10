using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps
{
    public class AppIdentity: IAppIdentity
    {
        public int ZoneId { get; }

        public int AppId { get; }

        public AppIdentity(int zoneId, int appId)
        {
            ZoneId = zoneId;
            AppId = appId;
        }
    }
}

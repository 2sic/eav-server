namespace ToSic.Eav.Apps
{
    public class AppZoneIds: IInAppAndZone
    {
        /// <inheritdoc />
        public int ZoneId { get; }

        /// <inheritdoc />
        public int AppId { get; }

        /// <summary>
        /// App identity containing zone/app combination
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        public AppZoneIds(int zoneId, int appId)
        {
            ZoneId = zoneId;
            AppId = appId;
        }

        public AppZoneIds(IInAppAndZone parent)
        {
            ZoneId = parent.ZoneId;
            AppId = parent.AppId;
        }
    }
}

namespace ToSic.Eav.Apps
{
    public class AppIdentity: IInAppAndZone
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
        public AppIdentity(int zoneId, int appId)
        {
            ZoneId = zoneId;
            AppId = appId;
        }

        public AppIdentity(IInAppAndZone parent)
        {
            ZoneId = parent.ZoneId;
            AppId = parent.AppId;
        }
    }
}

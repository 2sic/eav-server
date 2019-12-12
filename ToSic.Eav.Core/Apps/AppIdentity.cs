namespace ToSic.Eav.Apps
{
    public class AppIdentityTemp: IAppIdentity
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
        public AppIdentityTemp(int zoneId, int appId)
        {
            ZoneId = zoneId;
            AppId = appId;
        }

        public AppIdentityTemp(IAppIdentity parent)
        {
            ZoneId = parent.ZoneId;
            AppId = parent.AppId;
        }
    }
}

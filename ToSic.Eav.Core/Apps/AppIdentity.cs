using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// A full App-Identity.<br/>
    /// This is either used to pass identities around, or as a base class for more extensive objects which know their full identity. 
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public class AppIdentity: IAppIdentity
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

        public AppIdentity(IAppIdentity parent)
        {
            ZoneId = parent.ZoneId;
            AppId = parent.AppId;
        }
    }
}

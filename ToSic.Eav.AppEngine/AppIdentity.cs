using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps
{
    public class AppIdentity: HasLog, IAppIdentity
    {
        /// <inheritdoc />
        /// <summary>
        /// The zone id of this app
        /// </summary>
        public int ZoneId { get; }

        /// <inheritdoc />
        /// <summary>
        /// The app id
        /// </summary>
        public int AppId { get; }

        /// <summary>
        /// App identity containing zone/app combination
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <param name="parentLog">the current log - could be null if necessary</param>
        /// <param name="logKey">a log key because most inheriting objects will want their own key in the log</param>
        /// <param name="initialMessage"></param>
        public AppIdentity(int zoneId, int appId, Log parentLog, string logKey = null, string initialMessage = null) 
            : base(logKey ?? "App.Identy", parentLog, initialMessage ?? $"Zone {zoneId}, App {appId}")
        {
            ZoneId = zoneId;
            AppId = appId;
        }

        /// <summary>
        /// Text based identity for debugging only
        /// </summary>
        public string LogState => ZoneId + ":" + AppId;
    }
}

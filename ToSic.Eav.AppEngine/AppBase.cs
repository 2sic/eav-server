using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// Base object for things that have a full app-identity (app-id and zone-id)
    /// </summary>
    [PublicApi]
    public class AppBase: HasLog, IAppIdentity
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
        /// <param name="parentLog">the current log - could be null if necessary</param>
        /// <param name="logKey">a log key because most inheriting objects will want their own key in the log</param>
        /// <param name="initialMessage"></param>
        public AppBase(int zoneId, int appId, ILog parentLog, string logKey = null, string initialMessage = null) 
            : base(logKey ?? "App.Base", parentLog, initialMessage ?? $"Zone {zoneId}, App {appId}")
        {
            ZoneId = zoneId;
            AppId = appId;
        }

    }
}

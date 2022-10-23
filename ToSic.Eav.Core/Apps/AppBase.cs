using ToSic.Eav.Logging;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// Base object for things that have a full app-identity (app-id and zone-id) and can also log their state.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public abstract class AppBase: HasLog, IAppIdentity
    {
        /// <inheritdoc />
        public int ZoneId { get; private set; }

        /// <inheritdoc />
        public int AppId { get; private set; }

        /// <summary>
        /// DI Constructor - always run Init afterwards
        /// </summary>
        protected AppBase(string logName, CodeRef codeRef): base(logName ?? "App.Base", codeRef) { }

        /// <summary>
        /// App identity containing zone/app combination
        /// </summary>
        /// <param name="app">the identity</param>
        /// <param name="code">code-ref, must be created first</param>
        /// <param name="parentLog">the current log - could be null if necessary</param>
        protected AppBase Init(IAppIdentity app, CodeRef code, ILog parentLog)
        {
            InitLog(null, parentLog, null, code);
            ZoneId = app.ZoneId;
            AppId = app.AppId;
            return this;
        }
    }
}

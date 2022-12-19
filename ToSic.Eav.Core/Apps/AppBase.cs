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
        protected AppBase Init(IAppIdentity app)
        {
            var l = Log.Fn<AppBase>();
            ZoneId = app.ZoneId;
            AppId = app.AppId;
            return l.Return(this);
        }
    }
}

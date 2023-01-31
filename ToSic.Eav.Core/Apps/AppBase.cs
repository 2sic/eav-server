using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// Base object for things that have a full app-identity (app-id and zone-id) and can also log their state.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public abstract class AppBase: ServiceBase, IAppIdentity
    {
        /// <inheritdoc />
        public int ZoneId { get; private set; }

        /// <inheritdoc />
        public int AppId { get; private set; }

        /// <summary>
        /// DI Constructor - always run Init afterwards
        /// </summary>
        protected AppBase(string logName): base(logName ?? "App.Base") { }

        /// <summary>
        /// App identity containing zone/app combination
        /// </summary>
        /// <param name="app">the identity</param>
        protected AppBase Init(IAppIdentity app) => Log.Func(() =>
        {
            ZoneId = app.ZoneId;
            AppId = app.AppId;
            return this;
        });
    }
}

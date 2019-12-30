using System.Diagnostics;
using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repositories;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Persistence.Efc
{
    /// <summary>
    /// Will load all DB data into the memory data model using Entity Framework Core 1.1
    /// </summary>
    public partial class Efc11Loader: IRepositoryLoader
    {
        #region constructor and private vars
        public Efc11Loader(EavDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private readonly EavDbContext _dbContext;

        #endregion


        #region AppPackage

        /// <inheritdoc />
        /// <summary>Get Data to populate ICache</summary>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="entityIds">null or a List of EntityIds</param>
        /// <param name="parentLog"></param>
        /// <returns>app package with initialized app</returns>
        public AppState AppState(int appId, int[] entityIds = null, ILog parentLog = null)
        {
            var appIdentity = /*Factory.GetAppIdentity*/Apps.Apps.Identity(null, appId);
            return Update(new AppState(appIdentity, parentLog), AppStateLoadSequence.Start, entityIds, parentLog);
        }

        public AppState Update(AppState app, AppStateLoadSequence startAt, int[] entityIds = null, ILog parentLog = null)
        {
            app.Load(parentLog, () =>
            {
                Log = new Log("DB.EFLoad", app.Log, $"get app data package for a#{app.AppId}, " +
                                                    $"startAt: {startAt}, " +
                                                    $"ids only:{entityIds != null}");
                var wrapLog = Log.Call(useTimer: true);

                // prepare metadata lists & relationships etc.
                if (startAt <= AppStateLoadSequence.MetadataInit)
                    _sqlTotalTime = _sqlTotalTime.Add(InitMetadataLists(app, _dbContext));
                else
                    Log.Add("skipping metadata load");

                if (startAt <= AppStateLoadSequence.ContentTypeLoad && app.ContentTypesShouldBeReloaded)
                    startAt = AppStateLoadSequence.ContentTypeLoad;

                // prepare content-types
                if (startAt <= AppStateLoadSequence.ContentTypeLoad)
                {
                    var typeTimer = Stopwatch.StartNew();
                    app.InitContentTypes(ContentTypes(app.AppId, app));
                    typeTimer.Stop();
                    Log.Add($"timers types:{typeTimer.Elapsed}");
                }
                else
                    Log.Add("skipping content-type load");

                // load data
                if (startAt <= AppStateLoadSequence.ItemLoad)
                    LoadEntities(app, entityIds);
                else
                    Log.Add("skipping items load");

                Log.Add($"timers sql:sqlAll:{_sqlTotalTime}");
                wrapLog("ok");
            });
            return app;
        }

        #endregion

    }
}

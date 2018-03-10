using System.Diagnostics;
using ToSic.Eav.App;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc.Models;

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
        /// <param name="entityIds">null or a List of EntitiIds</param>
        /// <param name="parentLog"></param>
        /// <returns>app package with initialized app</returns>
        public AppDataPackage AppPackage(int appId, int[] entityIds = null, Log parentLog = null) 
            => Update(new AppDataPackage(appId, parentLog), AppPackageLoadingSteps.Start, entityIds, parentLog);

        public AppDataPackage Update(AppDataPackage app, AppPackageLoadingSteps startAt, int[] entityIds = null, Log parentLog = null)
        {
            Log = new Log("DB.EFLoad", parentLog, $"get app data package for a#{app.AppId}, " +
                                                  $"startAt: {startAt}, " +
                                                  $"ids only:{entityIds != null}");

            // prepare metadata lists & relationships etc.
            if (startAt <= AppPackageLoadingSteps.MetadataInit)
                _sqlTotalTime = _sqlTotalTime.Add(InitMetadataLists(app, _dbContext));
            else
                Log.Add("skipping metadata load");

            if (startAt <= AppPackageLoadingSteps.ContentTypeLoad && app.ContentTypesShouldBeReloaded)
                startAt = AppPackageLoadingSteps.ContentTypeLoad;

            // prepare content-types
            if (startAt <= AppPackageLoadingSteps.ContentTypeLoad)
            {
                var typeTimer = Stopwatch.StartNew();
                app.InitContentTypes(ContentTypes(app.AppId, app));
                typeTimer.Stop();
                Log.Add($"timers types:{typeTimer.Elapsed}");
            }
            else
                Log.Add("skipping content-type load");

            // load data
            if (startAt <= AppPackageLoadingSteps.ItemLoad)
                LoadEntities(app, entityIds);
            else
                Log.Add("skipping items load");

            Log.Add($"timers sql:sqlAll:{_sqlTotalTime}");

            app.LoadCompleted(); // tell app that loading is done
            Log.Add($"app dynamic load count: {app.DynamicUpdatesCount}");
            return app;
        }

        #endregion

    }
}

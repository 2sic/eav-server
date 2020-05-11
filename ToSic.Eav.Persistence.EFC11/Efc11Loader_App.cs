using System;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization;
using ToSic.Eav.Types;
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
        public Efc11Loader(EavDbContext dbContext, ILog testLog): this(dbContext)
        {
            Log = testLog;
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
            var appIdentity = State.Identity(null, appId);
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
                {
                    _sqlTotalTime = _sqlTotalTime.Add(InitMetadataLists(app, _dbContext));
                    // todo: 11.01 extensions
                    app.Path = PreloadAppPath(app.AppId);
                }
                else
                    Log.Add("skipping metadata load");

                if (startAt <= AppStateLoadSequence.ContentTypeLoad && app.ContentTypesShouldBeReloaded)
                    startAt = AppStateLoadSequence.ContentTypeLoad;

                // prepare content-types
                if (startAt <= AppStateLoadSequence.ContentTypeLoad)
                {
                    var typeTimer = Stopwatch.StartNew();
                    var dbTypes = ContentTypes(app.AppId, app);
                    dbTypes = TryToLoadFsTypesAndMerge(app.AppId, app.Path, dbTypes);
                    app.InitContentTypes(dbTypes);
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

        /// <summary>
        /// Must load the app-path from the settings early on, so that other loaders have it
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        private string PreloadAppPath(int appId)
        {
            var wrapLog = Log.Call<string>();
            try
            {
                var dbEntity = GetRawEntities(new int[0], appId, false, "2SexyContent-App");
                if (!dbEntity.Any()) return wrapLog("not in db", null);
                var json = dbEntity.FirstOrDefault()?.Json;
                if (string.IsNullOrEmpty(json)) return wrapLog("no json", null);

                Log.Add("app Entity found - this json: " + json);
                var serializer = Factory.Resolve<IDataDeserializer>();
                serializer.Initialize(0, ReflectionTypes.FakeCache.Values, null, Log);
                if (!(serializer.Deserialize(json, true, true) is Entity appEntity))
                    return wrapLog("can't deserialize", null);
                var path = appEntity.GetBestValue<string>("Folder");
                return string.IsNullOrWhiteSpace(path) 
                    ? wrapLog("no folder", null) 
                    : wrapLog(path, path);
            }
            catch (Exception ex)
            {
                // Ignore, but log
                Log.Add("error " + ex.Message);
            }

            return wrapLog("error", null);
        }

        #endregion

    }
}

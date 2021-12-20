using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Queries
{
    public class GlobalQueries: HasLog
    {
        public const string GlobalEavQueryPrefix = "Eav.Queries.Global.";
        public const string GlobalQueryPrefix = "Global.";

        #region Constructor / DI

        public GlobalQueries(/*Lazy<IRuntime> runtimeLazy,*/ IAppStates appStates, LogHistory logHistory): base(LogNames.Eav + ".GlbQry")
        {
            //_runtimeLazy = runtimeLazy;
            _appStates = appStates;
            _logHistory = logHistory;
        }
        //private readonly Lazy<IRuntime> _runtimeLazy;
        private readonly IAppStates _appStates;
        private readonly LogHistory _logHistory;

        #endregion

        /// <summary>
        /// List of global queries, caches after first scan
        /// </summary>
        /// <returns></returns>
        public List<IEntity> AllQueries()
        {
            if (_globalQueriesCache != null) return _globalQueriesCache;

            // add runtime stuff
            var runtimeType = QueriesInRuntime().ToList();

            // make sure it's case insensitive...
            _globalQueriesCache = runtimeType;
            return _globalQueriesCache;
        }
        private static List<IEntity> _globalQueriesCache;


        /// <summary>
        /// All content-types available in Reflection; will cache after first scan
        /// </summary>
        /// <returns></returns>
        private List<IEntity> QueriesInRuntime()
        {
            if (_runtimeCache != null) return _runtimeCache;

            Log.Add("Load Global Queries");
            _logHistory.Add(LogNames.LogHistoryGlobalTypes, Log);
            var wrapLog = Log.Call<List<IEntity>>();

            var globalApp = _appStates.Get(Constants.PresetIdentity);
            var queries = globalApp.List.Where(e => e.Type.Is(Constants.QueryTypeName)).ToList();
            _runtimeCache = queries;
            //var runtime = _runtimeLazy.Value.Init(Log);
            //_runtimeCache = runtime?.LoadGlobalItems("query")?.ToList() ?? new List<IEntity>();
            return wrapLog($"{_runtimeCache?.Count}", _runtimeCache);
        }

        private static List<IEntity> _runtimeCache;


        public IEntity FindQuery(string name)
        {
            // Note: must use GetBestValue("Name") as GetTitle() will fail, since the type is often not known
            var types = _globalQueriesCache ?? AllQueries();
            return types.FirstOrDefault(e => e.Value<string>("Name") == name);
        }
    }
}

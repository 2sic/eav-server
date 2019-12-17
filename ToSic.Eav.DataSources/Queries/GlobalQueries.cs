using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Run;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Queries
{
    public static class GlobalQueries
    {
        public const string GlobalQueryPrefix = "Eav.Queries.Global.";

        /// <summary>
        /// List of global queries, caches after first scan
        /// </summary>
        /// <returns></returns>
        public static List<IEntity> AllQueries()
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
        private static IEnumerable<IEntity> QueriesInRuntime()
        {
            if (_runtimeCache != null) return _runtimeCache;

            var runtime = Factory.Resolve<IRuntime>();
            _runtimeCache = runtime?.LoadGlobalItems("query") ?? new List<IEntity>();
            return _runtimeCache;
        }

        private static IEnumerable<IEntity> _runtimeCache;


        public static IEntity FindQuery(string name)
        {
            // Note: must use GetBestValue("Name") as GetTitle() will fail, since the type is often not known
            var types = _globalQueriesCache ?? AllQueries();
            return types.FirstOrDefault(e => e.GetBestValue("Name").ToString() == name);
        }
    }
}

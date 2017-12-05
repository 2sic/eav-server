using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources.Queries
{
    public class Global
    {
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
            // use the types which have been loaded
            // this is to enable lookup of system types, while in the background we're still building the json-types
            // this is important, because the deserializer for json will also call this
            var types = _globalQueriesCache ?? AllQueries();
            return types.FirstOrDefault(e => e.GetBestValue("Name").ToString() == name);
        }
    }
}

using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    public partial class App
    {
        /// <summary>
        /// Cached list of queries
        /// </summary>
        [PrivateApi]
        protected IDictionary<string, IQuery> Queries;

        /// <summary>
        /// Accessor to queries. Use like:
        /// - App.Query.Count
        /// - App.Query.ContainsKey(...)
        /// - App.Query["One Event"].List
        /// </summary>
        /// <inheritdoc />
        public IDictionary<string, IQuery> Query
        {
            get
            {
                if (Queries != null) return Queries;

                if (ConfigurationProvider == null)
                    throw new Exception("Can't use app-queries, because the necessary configuration provider hasn't been initialized. Call InitData first.");
                var queryMan = new QueryManager(DataSourceFactory).Init(Log);
                Queries = queryMan.AllQueries(this, ConfigurationProvider, ShowDrafts);
                return Queries;
            }
        }

        /// <inheritdoc />
        public Query GetQuery(string name)
        {
            if (name.StartsWith(GlobalQueries.GlobalEavQueryPrefix))
                return GetGlobalQuery(name);

            if (name.StartsWith(GlobalQueries.GlobalQueryPrefix))
                return GetGlobalQuery(name.Substring(GlobalQueries.GlobalQueryPrefix.Length -1));

            // Try to find the query, abort if not found
            if (Query.ContainsKey(name) && Query[name] is Query query)
                return query;

            // not found
            return null;
        }

        private Query GetGlobalQuery(string name)
        {
            var qent = _dependencies.GlobalQueriesLazy.Value.FindQuery(name) 
                ?? throw new Exception($"can't find global query {name}");
            return new Query(DataSourceFactory).Init(ZoneId, AppId, qent, ConfigurationProvider, ShowDrafts, null, Log);
        }
    }
}

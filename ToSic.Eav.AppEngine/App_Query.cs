using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Queries;

namespace ToSic.Eav.Apps
{
    public partial class App
    {
        /// <summary>
        /// Cached list of queries
        /// </summary>
        protected IDictionary<string, IDataSource> Queries;

        /// <summary>
        /// Accessor to queries. Use like:
        /// - App.Query.Count
        /// - App.Query.ContainsKey(...)
        /// - App.Query["One Event"].List
        /// </summary>
        public IDictionary<string, IDataSource> Query
        {
            get
            {
                if (Queries != null) return Queries;

                if (ConfigurationProvider == null)
                    throw new Exception("Can't use app-queries, because the necessary configuration provider hasn't been initialized. Call InitData first.");
                Queries = QueryManager.AllQueries(ZoneId, AppId, ConfigurationProvider, Log, ShowDrafts);
                return Queries;
            }
        }

        public DeferredQuery GetQuery(string name)
        {
            if (name.StartsWith(Global.GlobalQueryPrefix))
                return GetGlobalQuery(name);

            // Try to find the query, abort if not found
            if (Query.ContainsKey(name) && Query[name] is DeferredQuery query)
                return query;

            // not found
            return null;
        }

        private DeferredQuery GetGlobalQuery(string name)
        {
            var qent = Global.FindQuery(name) 
                ?? throw new Exception($"can't find global query {name}");
            return new DeferredQuery(ZoneId, AppId, qent, ConfigurationProvider, ShowDrafts);
        }
    }
}

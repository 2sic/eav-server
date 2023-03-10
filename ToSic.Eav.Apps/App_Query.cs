using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources.Queries;
using static ToSic.Eav.DataSources.DataSourceConstants;

namespace ToSic.Eav.Apps
{
    public partial class App
    {
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
                if (_queries != null) return _queries;

                if (ConfigurationProvider == null)
                    throw new Exception("Can't use app-queries, because the necessary configuration provider hasn't been initialized. Call InitData first.");
                return _queries = Services.QueryManager.Value.AllQueries(this, ConfigurationProvider, ShowDrafts);
            }
        }
        private IDictionary<string, IQuery> _queries;

        /// <inheritdoc />
        public Query GetQuery(string name)
        {
            // Try to find the local query, abort if not found
            // Doing this first starting in v13 (previously this happened last)
            // Not final implementation, but goal is to properly cascade from AppState to parent
            if (Query.ContainsKey(name) && Query[name] is Query query)
                return query;

            if (name.StartsWith(GlobalQueryPrefix))
                return GetParentOrGlobalQuery(name.Substring(GlobalQueryPrefix.Length - 1),
                    $"Query with prefix {GlobalQueryPrefix} couldn't be found.");

            if (name.StartsWith(SystemQueryPrefixPreV15) || name.StartsWith(SystemQueryPrefix))
                return GetParentOrGlobalQuery(name, "Global EAV Query not Found!");

            return GetParentOrGlobalQuery(name, null);
        }

        private Query GetParentOrGlobalQuery(string name, string errorOnNotFound)
        {
            var qEntity = Services.QueryManager.Value.FindQuery(AppState, name, recurseParents: 3);

            if (qEntity == null && errorOnNotFound != null) throw new Exception(errorOnNotFound);

            return Services.QueryGenerator.New().Init(ZoneId, AppId, qEntity, ConfigurationProvider, ShowDrafts, null);
        }
    }
}

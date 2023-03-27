using System;
using System.Collections.Generic;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Query;
using static ToSic.Eav.DataSource.DataSourceConstants;

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
                return _queries = Services.QueryManager.Value.AllQueries(this, ConfigurationProvider);
            }
        }
        private IDictionary<string, IQuery> _queries;

        /// <inheritdoc />
        public Query GetQuery(string name)
        {
            // Try to find the local query, abort if not found
            // Not final implementation, but goal is to properly cascade from AppState to parent
            if (Query.ContainsKey(name) && Query[name] is Query query)
                return query;

            // #dropGlobalQueryPrefixWithUnknownPurpose - delete code 2023 Q3
            //if (name.StartsWith(GlobalQueryPrefix))
            //    return GetParentOrGlobalQuery(name.Substring(GlobalQueryPrefix.Length - 1),
            //        $"Query with prefix {GlobalQueryPrefix} couldn't be found.");

            // Try to find query definition - while also checking parent apps
            var qEntity = Services.QueryManager.Value.FindQuery(AppState, name, recurseParents: 3);
            if (qEntity == null && name.StartsWith(SystemQueryPrefixPreV15) || name.StartsWith(SystemQueryPrefix))
                throw new Exception("Global EAV Query not Found!");

            return Services.QueryGenerator.New().Init(ZoneId, AppId, qEntity, ConfigurationProvider);
        }

        // #dropGlobalQueryPrefixWithUnknownPurpose - delete code 2023 Q3
        ///// <summary>
        ///// Unsure what this is for, and if there are actually any queries that match this!
        ///// it appears to not be part of the name (seems to get removed) but a key to look in parents - probably drop
        ///// </summary>
        //// TODO: DROP FUNCTIONALITY, SEE IF anyone is affected - I assume it was internal only, a long time ago.
        //public const string GlobalQueryPrefix = "Global.";

    }
}

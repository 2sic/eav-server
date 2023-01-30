using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Queries;

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
                return _queries = Deps.QueryManager.Value.AllQueries(this, ConfigurationProvider, ShowDrafts);
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

            if (name.StartsWith(DataSourceConstants.GlobalQueryPrefix))
                return GetParentOrGlobalQuery(name.Substring(DataSourceConstants.GlobalQueryPrefix.Length - 1),
                    $"Query with prefix {DataSourceConstants.GlobalQueryPrefix} couldn't be found.");

            if (name.StartsWith(DataSourceConstants.GlobalEavQueryPrefix))
                return GetParentOrGlobalQuery(name, "Global EAV Query not Found!");

            return GetParentOrGlobalQuery(name, null);

            // not found
            //return null;
        }

        private Query GetParentOrGlobalQuery(string name, string errorOnNotFound)
        {
            // try parent
            var parentAppState = AppState.ParentApp?.AppState;
            var qent = parentAppState == null ? null : Deps.QueryManager.Value.FindQuery(parentAppState, name);

            // for inherited app, parent-app is master shared app, so we need parent-parent-app to check the global queries
            if (qent == null && parentAppState.ParentApp?.AppState != null)
                qent = Deps.QueryManager.Value.FindQuery(parentAppState.ParentApp.AppState, name);

            if (qent == null && errorOnNotFound != null) throw new Exception(errorOnNotFound);

            return Deps.QueryGenerator.New().Init(ZoneId, AppId, qent, ConfigurationProvider, ShowDrafts, null);
        }
    }
}

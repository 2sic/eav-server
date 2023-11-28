using System;
using System.Collections.Generic;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Query;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.Apps;

partial class App
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
        if (Query.TryGetValue(name, out var value) && value is Query query)
            return query;

        // Try to find query definition - while also checking parent apps
        var qEntity = Services.QueryManager.Value.FindQuery(AppStateInt, name, recurseParents: 3);
            
        if (qEntity != null)
            return Services.QueryGenerator.New().Init(ZoneId, AppId, qEntity, ConfigurationProvider);

        var isGlobal = name.StartsWith(SystemQueryPrefixPreV15) || name.StartsWith(SystemQueryPrefix);
        throw new Exception((isGlobal ? "Global " : "") + "Query not Found!");

    }
}
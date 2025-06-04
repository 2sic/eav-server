using ToSic.Eav.Apps;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Data.Entities.Sys.Lists;
using ToSic.Lib.LookUp.Engines;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSource.Internal.Query;

/// <summary>
/// Helpers to work with Data Queries
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryManager(
    Generator<Query> queryGenerator,
    LazySvc<IAppReaderFactory> appReaders,
    LazySvc<QueryDefinitionBuilder> queryDefBuilder)
    : ServiceBase($"{DataSourceConstantsInternal.LogPrefix}.QryMan", connect: [queryGenerator, appReaders, queryDefBuilder])
{
    /// <summary>
    /// Get a query definition from the current app
    /// </summary>
    public QueryDefinition Get(IAppIdentity appIdentity, int queryId)
    {
        var l = Log.Fn<QueryDefinition>($"{nameof(queryId)}:{queryId}");
        var app = appReaders.Value.GetOrKeep(appIdentity);
        var qEntity = GetQueryEntity(queryId, app);
        var qDef = queryDefBuilder.Value.Create(qEntity, app.AppId);
        return l.Return(qDef);
    }


    /// <summary>
    /// Get an Entity Describing a Query
    /// </summary>
    /// <param name="entityId">EntityId</param>
    /// <param name="appReaderOrId">DataSource to load Entity from</param>
    internal IEntity GetQueryEntity(int entityId, IAppIdentity appReaderOrId)
    {
        var l = Log.Fn<IEntity>($"{entityId}");
        var app = appReaders.Value.GetOrKeep(appReaderOrId);
        try
        {
            var queryEntity = app.List.FindRepoId(entityId);
            if (queryEntity.Type.NameId != QueryConstants.QueryTypeName)
                throw new ArgumentException("Entity is not an DataQuery Entity", nameof(entityId));
            return l.Return(queryEntity);
        }
        catch (Exception ex)
        {
            l.Ex(new ArgumentException($"Could not load Query-Entity with ID {entityId}.", nameof(entityId)));
            l.Ex(ex);
            throw;
        }
    }

    /// <summary>
    /// Assembles a list of all queries / Queries configured for this app. 
    /// The queries internally are not assembled yet for performance reasons...
    /// ...but will be auto-assembled the moment they are accessed
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, IQuery> AllQueries(IAppIdentity app, ILookUpEngine lookUps)
    {
        var l = Log.Fn<Dictionary<string, IQuery>>();
        var dict = new Dictionary<string, IQuery>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var entQuery in AllQueryItems(app))
        {
            var delayedQuery = queryGenerator.New().Init(app.ZoneId, app.AppId, entQuery, lookUps);
            // make sure it doesn't break if two queries have the same name...
            var name = entQuery.GetBestTitle();
            if (!dict.ContainsKey(name))
                dict[name] = delayedQuery;
        }
        return l.Return(dict);
    }

    internal IImmutableList<IEntity> AllQueryItems(IAppIdentity appIdOrReader, int recurseParents = 0)
    {
        var l = Log.Fn<IImmutableList<IEntity>>($"App: {appIdOrReader.AppId}, recurse: {recurseParents}");
        var appReader = appReaders.Value.GetOrKeep(appIdOrReader);
        var queries = appReader.List.OfType(QueryConstants.QueryTypeName).ToImmutableList();

        if (recurseParents <= 0)
            return l.Return(queries, "ok, no recursions");

        l.A($"Try to recurse parents {recurseParents}");
        var parentAppState = appReader.GetParentCache();
        if (parentAppState == null)
            return l.Return(queries, "no more parents to recurse on");

        var parentQueries = AllQueryItems(parentAppState, recurseParents -1);
        queries = [.. queries, .. parentQueries];
        return l.Return(queries, "ok");
    }

    public IQuery? GetQuery(IAppIdentity appIdentity, string? nameOrGuid, ILookUpEngine lookUps, int recurseParents = 0)
    {
        var l = Log.Fn<IQuery>($"{nameOrGuid}, recurse: {recurseParents}");
        var qEntity = FindQuery(appIdentity, nameOrGuid, recurseParents);
        if (qEntity == null)
            return l.ReturnNull("not found");
        var delayedQuery = queryGenerator.New().Init(appIdentity.ZoneId, appIdentity.AppId, qEntity, lookUps);
        return l.Return(delayedQuery, "found");
    }

    internal IEntity? FindQuery(IAppIdentity appIdentity, string? nameOrGuid, int recurseParents = 0) 
    {
        var l = Log.Fn<IEntity?>($"{nameOrGuid}, recurse: {recurseParents}");
        if (nameOrGuid.IsEmptyOrWs())
            return l.ReturnNull("null - no name");
        var all = AllQueryItems(appIdentity, recurseParents);
        var result = FindByNameOrGuid(all, nameOrGuid);
        // If nothing found, and we have an old name, try the new name
        if (result == null && nameOrGuid.StartsWith(DataSourceConstantsInternal.SystemQueryPrefixPreV15))
            result = FindByNameOrGuid(all, nameOrGuid.Replace(DataSourceConstantsInternal.SystemQueryPrefixPreV15, DataSourceConstantsInternal.SystemQueryPrefix));
        return l.Return(result, result == null ? "null" : "ok");
    }


    private static IEntity? FindByNameOrGuid(IImmutableList<IEntity> queries, string nameOrGuid) =>
        queries.FirstOrDefault(q =>
            q.Get<string>("Name").EqualsInsensitive(nameOrGuid)
            || q.EntityGuid.ToString().EqualsInsensitive(nameOrGuid)
        );
}
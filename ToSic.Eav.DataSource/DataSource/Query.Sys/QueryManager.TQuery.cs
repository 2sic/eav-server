using ToSic.Eav.Apps;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.LookUp.Sys.Engines;

namespace ToSic.Eav.DataSource.Query.Sys;

/// <summary>
/// Helpers to work with Data Queries.
/// This is a generic manager, since we may need it for classic queries or for TypedQueries within 2sxc.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryManager<TQuery>(
    Generator<TQuery> queryGenerator,
    LazySvc<IAppReaderFactory> appReaders,
    LazySvc<QueryDefinitionFactory> queryDefBuilder)
    : ServiceBase($"{DataSourceConstantsInternal.LogPrefix}.QryMan", connect: [queryGenerator, appReaders, queryDefBuilder])
    where TQuery : Query
{
    /// <summary>
    /// Get a query definition from the current app
    /// </summary>
    public QueryDefinition GetDefinition(IAppIdentity appIdentity, int queryId)
    {
        var l = Log.Fn<QueryDefinition>($"{nameof(queryId)}:{queryId}");
        var app = appReaders.Value.GetOrKeep(appIdentity);
        var qEntity = GetQueryEntityOrThrow(app, queryId);
        return l.Return(GetDefinition(app.AppId, qEntity));
    }

    public QueryDefinition GetDefinition(int appId, int queryId) =>
        GetDefinition(appReaders.Value.AppIdentity(appId), queryId);

    public QueryDefinition GetDefinition(int appId, IEntity entity) =>
        queryDefBuilder.Value.Create(appId, entity);


    /// <summary>
    /// Get an Entity Describing a Query
    /// </summary>
    /// <param name="appReaderOrId">DataSource to load Entity from</param>
    /// <param name="entityId">EntityId</param>
    private IEntity GetQueryEntityOrThrow(IAppIdentity appReaderOrId, int entityId)
    {
        var l = Log.Fn<IEntity>($"{entityId}");
        var app = appReaders.Value.GetOrKeep(appReaderOrId);
        try
        {
            var queryEntity = app.List.FindRepoId(entityId);
            return queryEntity?.Type.NameId == QueryDefinition.TypeName
                ? l.Return(queryEntity)
                : throw new ArgumentException(@"Entity is not an DataQuery Entity", nameof(entityId));
        }
        catch (Exception ex)
        {
            l.Ex(new ArgumentException($@"Could not load Query-Entity with ID {entityId}.", nameof(entityId)));
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
        foreach (var entQuery in AllQueryEntities(app))
        {
            var delayedQuery = queryGenerator.New();
            delayedQuery.Init(app.ZoneId, app.AppId, entQuery, lookUps);
            // make sure it doesn't break if two queries have the same name...
            var name = entQuery.GetBestTitle();
            // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd
            if (name != null && !dict.ContainsKey(name))
                dict[name] = delayedQuery;
        }
        return l.Return(dict);
    }

    internal IImmutableList<IEntity> AllQueryEntities(IAppIdentity appIdOrReader, int recurseParents = 0)
    {
        var l = Log.Fn<IImmutableList<IEntity>>($"App: {appIdOrReader.AppId}, recurse: {recurseParents}");
        var appReader = appReaders.Value.GetOrKeep(appIdOrReader)!;
        var queries = appReader.List
            .GetAll(QueryDefinition.TypeName)
            .ToImmutableOpt();

        if (recurseParents <= 0)
            return l.Return(queries, "ok, no recursions");

        l.A($"Try to recurse parents {recurseParents}");
        var parentAppState = appReader.GetParentCache();
        if (parentAppState == null)
            return l.Return(queries, "no more parents to recurse on");

        var parentQueries = AllQueryEntities(parentAppState, recurseParents -1);
        queries = [.. queries, .. parentQueries];
        return l.Return(queries, "ok");
    }

    public TQuery? TryGetQuery(IAppIdentity appIdentity, string? nameOrGuid, ILookUpEngine lookUps, int recurseParents = 0)
    {
        var l = Log.Fn<TQuery>($"{nameOrGuid}, recurse: {recurseParents}");
        var qEntity = TryGetQueryEntity(appIdentity, nameOrGuid, recurseParents);
        if (qEntity == null)
            return l.ReturnNull("not found");
        var delayedQuery = queryGenerator.New();
        delayedQuery.Init(appIdentity.ZoneId, appIdentity.AppId, qEntity, lookUps);
        return l.Return(delayedQuery, "found");
    }

    internal IEntity? TryGetQueryEntity(IAppIdentity appIdentity, string? nameOrGuid, int recurseParents = 0) 
    {
        var l = Log.Fn<IEntity?>($"{nameOrGuid}, recurse: {recurseParents}");

        if (nameOrGuid.IsEmptyOrWs())
            return l.ReturnNull("null - no name");

        var all = AllQueryEntities(appIdentity, recurseParents);
        var result = TryFindByNameOrGuid(all, nameOrGuid);

        // If nothing found, and we have an old name, try the new name
        if (result == null && nameOrGuid.StartsWith(DataSourceConstantsInternal.SystemQueryPrefixPreV15))
            throw new($"Found a query beginning with {DataSourceConstantsInternal.SystemQueryPrefixPreV15} which is not supported any more. " +
                      $"Use the prefix {DataSourceConstantsInternal.SystemQueryPrefix}. The query was: '{nameOrGuid}'");

        return l.Return(result, result == null ? "null" : "ok");
    }


    private static IEntity? TryFindByNameOrGuid(IImmutableList<IEntity> queries, string nameOrGuid) =>
        queries.FirstOrDefault(q =>
            q.Get<string>("Name").EqualsInsensitive(nameOrGuid)
            || q.EntityGuid.ToString().EqualsInsensitive(nameOrGuid)
        );
}
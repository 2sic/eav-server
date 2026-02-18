using ToSic.Eav.Apps;
using ToSic.Eav.LookUp.Sys.Engines;

namespace ToSic.Eav.DataSource.Query.Sys;

/// <summary>
/// Helpers to work with Data Queries.
/// This is a generic manager, since we may need it for classic queries or for TypedQueries within 2sxc.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryManager<TQuery>(QueryDefinitionService queryDefSvc, Generator<TQuery> queryGenerator)
    : ServiceBase($"{DataSourceConstantsInternal.LogPrefix}.QryMan", connect: [queryDefSvc, queryGenerator])
    where TQuery : Query
{

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
        foreach (var entQuery in queryDefSvc.AllQueryEntities(app))
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
    
    public TQuery? TryGetQuery(IAppIdentity appIdentity, string? nameOrGuid, ILookUpEngine lookUps, int recurseParents = 0)
    {
        var l = Log.Fn<TQuery>($"{nameOrGuid}, recurse: {recurseParents}");
        var qEntity = queryDefSvc.TryGetQueryEntity(appIdentity, nameOrGuid, recurseParents);
        if (qEntity == null)
            return l.ReturnNull("not found");
        var delayedQuery = queryGenerator.New();
        delayedQuery.Init(appIdentity.ZoneId, appIdentity.AppId, qEntity, lookUps);
        return l.Return(delayedQuery, "found");
    }

}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Services;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;
using static ToSic.Eav.DataSource.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Queries
{
	/// <summary>
	/// Helpers to work with Data Queries
	/// </summary>
	[PrivateApi]
	public class QueryManager: ServiceBase
	{
        private readonly LazySvc<IAppStates> _appStates;
        private readonly Generator<Query> _queryGenerator;
        public IDataSourcesService DataSourceFactory { get; }

        public QueryManager(IDataSourcesService dataSourceFactory, Generator<Query> queryGenerator, LazySvc<IAppStates> appStates) : base($"{LogPrefix}.QryMan")
        {
            ConnectServices(
                DataSourceFactory = dataSourceFactory,
                _queryGenerator = queryGenerator,
                _appStates = appStates
            );
        }

        /// <summary>
        /// Get an Entity Describing a Query
        /// </summary>
        /// <param name="entityId">EntityId</param>
        /// <param name="dataSource">DataSource to load Entity from</param>
        internal IEntity GetQueryEntity(int entityId, AppState dataSource) => Log.Func($"{entityId}", l =>
        {
            var wrapLog = Log.Fn<IEntity>($"{entityId}");
            try
            {
                var queryEntity = dataSource.List.FindRepoId(entityId);
                if (queryEntity.Type.NameId != QueryConstants.QueryTypeName)
                    throw new ArgumentException("Entity is not an DataQuery Entity", nameof(entityId));
                return wrapLog.Return(queryEntity);
            }
            catch (Exception ex)
            {
                l.Ex(new ArgumentException($"Could not load Query-Entity with ID {entityId}.", nameof(entityId)));
                l.Ex(ex);
                throw;
            }

        });

        /// <summary>
        /// Assembles a list of all queries / Queries configured for this app. 
        /// The queries internally are not assembled yet for performance reasons...
        /// ...but will be auto-assembled the moment they are accessed
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, IQuery> AllQueries(IAppIdentity app, ILookUpEngine lookUps) => Log.Func(() =>
        {
            var dict = new Dictionary<string, IQuery>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var entQuery in AllQueryItems(app))
            {
                var delayedQuery = _queryGenerator.New().Init(app.ZoneId, app.AppId, entQuery, lookUps);
                // make sure it doesn't break if two queries have the same name...
                var name = entQuery.GetBestTitle();
                if (!dict.ContainsKey(name))
                    dict[name] = delayedQuery;
            }
            return (dict);
        });

        internal IImmutableList<IEntity> AllQueryItems(IAppIdentity app, int recurseParents = 0) => Log.Func($"App: {app.AppId}, recurse: {recurseParents}", l =>
        {
            // TODO
            var appState = app as AppState ?? _appStates.Value.Get(app);
            var result = QueryEntities(appState);
            if (recurseParents <= 0) return (result, "ok, no recursions");
            l.A($"Try to recurse parents {recurseParents}");
            if (appState.ParentApp?.AppState == null) return (result, "no more parents to recurse on");
            var resultFromParents = AllQueryItems(appState.ParentApp.AppState, recurseParents -1);
            result = result.Concat(resultFromParents).ToImmutableList();
            return (result, "ok");
        });

        internal IEntity FindQuery(IAppIdentity appIdentity, string nameOrGuid, int recurseParents = 0) => Log.Func($"{nameOrGuid}, recurse: {recurseParents}", () =>
        {
            if (nameOrGuid.IsEmptyOrWs()) return (null, "null - no name");
            var all = AllQueryItems(appIdentity, recurseParents);
            var result = FindByNameOrGuid(all, nameOrGuid);
            // If nothing found and we have an old name, try the new name
            if (result == null && nameOrGuid.StartsWith(SystemQueryPrefixPreV15))
                result = FindByNameOrGuid(all, nameOrGuid.Replace(SystemQueryPrefixPreV15, SystemQueryPrefix));
            return (result, result == null ? "null" : "ok");
        });

        // todo: move to query-read or helper

        private static IImmutableList<IEntity> QueryEntities(AppState appState)
            => appState.List.OfType(QueryConstants.QueryTypeName).ToImmutableList();


        // todo: move to query-read or helper, or make private

        private static IEntity FindByNameOrGuid(IImmutableList<IEntity> queries, string nameOrGuid) =>
            queries.FirstOrDefault(
                q => q.Value<string>("Name").EqualsInsensitive(nameOrGuid)
                     || q.EntityGuid.ToString().EqualsInsensitive(nameOrGuid));
    }
}

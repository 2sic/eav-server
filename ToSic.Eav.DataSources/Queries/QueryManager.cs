using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.LookUp;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;
using static System.StringComparison;
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
        public DataSourceFactory DataSourceFactory { get; }

        public QueryManager(DataSourceFactory dataSourceFactory, Generator<Query> queryGenerator, LazySvc<IAppStates> appStates) : base($"{DataSourceConstants.LogPrefix}.QryMan")
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
                if (queryEntity.Type.NameId != Constants.QueryTypeName)
                    throw new ArgumentException("Entity is not an DataQuery Entity", nameof(entityId));
                return (queryEntity);
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
        internal Dictionary<string, IQuery> AllQueries(IAppIdentity app, ILookUpEngine valuesCollectionProvider, bool showDrafts
        ) => Log.Func($"..., ..., {showDrafts}", () =>
        {
            var dict = new Dictionary<string, IQuery>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var entQuery in AllQueryItems(app))
            {
                var delayedQuery = _queryGenerator.New().Init(app.ZoneId, app.AppId, entQuery, valuesCollectionProvider,
                    showDrafts, null);
                // make sure it doesn't break if two queries have the same name...
                var name = entQuery.GetBestTitle();
                if (!dict.ContainsKey(name))
                    dict[name] = delayedQuery;
            }

            return (dict);
        });

        internal IImmutableList<IEntity> AllQueryItems(IAppIdentity app) => Log.Func(() =>
        {
            // TODO
            var appState = _appStates.Value.Get(app);
            var result = QueryEntities(appState);
            return (result, "ok");
        });

        internal IEntity FindQuery(IAppIdentity appIdentity, string nameOrGuid) => Log.Func(nameOrGuid, () =>
        {
            var all = AllQueryItems(appIdentity);
            var result = FindByNameOrGuid(all, nameOrGuid);
            return (result, result == null ? "null" : "ok");
        });

        public static IImmutableList<IEntity> QueryEntities(AppState appState)
            => appState.List.OfType(Constants.QueryTypeName).ToImmutableList();

        //public static IEntity FindByName(IImmutableList<IEntity> queries, string name) 
        //    => queries.FirstOrDefault(e => e.Value<string>("Name") == name);

        public static IEntity FindByNameOrGuid(IImmutableList<IEntity> queries, string nameOrGuid) =>
            queries.FirstOrDefault(
                q => string.Equals(q.Value<string>("Name"), nameOrGuid, InvariantCultureIgnoreCase)
                     || string.Equals(q.EntityGuid.ToString(), nameOrGuid, InvariantCultureIgnoreCase));
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;
using static System.StringComparison;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Queries
{
	/// <summary>
	/// Helpers to work with Data Queries
	/// </summary>
	[PrivateApi]
	public class QueryManager: HasLog<QueryManager>
	{
        public DataSourceFactory DataSourceFactory { get; }

        public QueryManager(DataSourceFactory dataSourceFactory): base($"{DataSourceConstants.LogPrefix}.QryMan")
        {
            DataSourceFactory = dataSourceFactory.Init(Log);
        }

	    /// <summary>
		/// Get an Entity Describing a Query
		/// </summary>
		/// <param name="entityId">EntityId</param>
		/// <param name="dataSource">DataSource to load Entity from</param>
		internal IEntity GetQueryEntity(int entityId, AppState dataSource)
        {
            var wrapLog = Log.Call<IEntity>($"{entityId}");
			try
			{
			    var queryEntity = dataSource.List.FindRepoId(entityId);
                if (queryEntity.Type.StaticName != Constants.QueryTypeName)
                    throw new ArgumentException("Entity is not an DataQuery Entity", nameof(entityId));
			    return wrapLog("ok", queryEntity);
			}
			catch (Exception)
            {
                wrapLog("error", null);
				throw new ArgumentException($"Could not load Query-Entity with ID {entityId}.", nameof(entityId));
			}

		}

        /// <summary>
        /// Assembles a list of all queries / Queries configured for this app. 
        /// The queries internally are not assembled yet for performance reasons...
        /// ...but will be auto-assembled the moment they are accessed
        /// </summary>
        /// <returns></returns>
	    internal Dictionary<string, IQuery> AllQueries(IAppIdentity app, ILookUpEngine valuesCollectionProvider, bool showDrafts)
        {
            var wrapLog = Log.Call<Dictionary<string, IQuery>>($"..., ..., {showDrafts}");
	        var dict = new Dictionary<string, IQuery>(StringComparer.InvariantCultureIgnoreCase);
	        foreach (var entQuery in AllQueryItems(app))
	        {
	            var delayedQuery = new Query(DataSourceFactory).Init(app.ZoneId, app.AppId, entQuery, valuesCollectionProvider, showDrafts, null, Log);
                // make sure it doesn't break if two queries have the same name...
	            var name = entQuery.Title[0].ToString();
	            if (!dict.ContainsKey(name))
	                dict[name] = delayedQuery;
	        }

            return wrapLog("ok", dict);
        }

	    internal IImmutableList<IEntity> AllQueryItems(IAppIdentity app)
        {
            var wrapLog = Log.Call<IImmutableList<IEntity>>();
            var appState = DataSourceFactory.ServiceProvider.Build<IAppStates>().Get(app);
            var result = QueryEntities(appState);
            return wrapLog("ok", result);
        }

        internal IEntity FindQuery(IAppIdentity appIdentity, string nameOrGuid)
        {
            var wrapLog = Log.Call<IEntity>(nameOrGuid);
            var all = AllQueryItems(appIdentity);
            var result = FindByNameOrGuid(all, nameOrGuid);
            return wrapLog(result == null ? "null" : "ok", result);
        }

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

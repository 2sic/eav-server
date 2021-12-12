using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.DataSources.Queries
{
	/// <summary>
	/// Factory to create a Data Query
	/// </summary>
	public class QueryBuilder: HasLog<QueryBuilder>
	{
        #region Dependency Injection

        /// <summary>
		/// DI Constructor
		/// </summary>
		/// <remarks>
		/// Never call this constructor from your code, as it re-configures the DataSourceFactory it gets
		/// </remarks>
        public QueryBuilder(DataSourceFactory dataSourceFactory, IZoneCultureResolver cultureResolver, IAppStates appStates) : base("DS.PipeFt")
        {
            _cultureResolver = cultureResolver;
            _appStates = appStates;
            _dataSourceFactory = dataSourceFactory.Init(Log);
            _dataSourceFactory.Init(Log);
        }

        private readonly DataSourceFactory _dataSourceFactory;
        private readonly IZoneCultureResolver _cultureResolver;
        private readonly IAppStates _appStates;
        

        #endregion

		/// <summary>
        /// Build a query-definition object based on the entity-ID defining the query
        /// </summary>
        /// <returns></returns>
        public QueryDefinition GetQueryDefinition(int appId, int queryEntityId)
	    {
            var wrapLog = Log.Call($"def#{queryEntityId} for a#{appId}");

	        try
            {
                var app = _appStates.IdentityOfApp(appId);
                var source = _dataSourceFactory.GetPublishing(app);
                var appEntities = source.List;

                // use findRepo, as it uses the cache, which gives the list of all items
	            var dataQuery = appEntities.FindRepoId(queryEntityId);
	            var result = new QueryDefinition(dataQuery, appId, Log);
                wrapLog(null);
                return result;
            }
	        catch (KeyNotFoundException)
	        {
	            throw new Exception("QueryEntity not found with ID " + queryEntityId + " on AppId " + appId);
	        }

	    }

        public const string ConfigKeyPartSettings = "settings";
	    public const string ConfigKeyPipelineSettings = "pipelinesettings";


	    public Tuple<IDataSource, Dictionary<string, IDataSource>> BuildQuery(QueryDefinition queryDef,
            ILookUpEngine lookUpEngineToClone,
            IEnumerable<ILookUp> overrideLookUps,
            bool showDrafts)
        {
	        #region prepare shared / global value providers

	        overrideLookUps = overrideLookUps?.ToList();
            var wrapLog = Log.Call<Tuple<IDataSource, Dictionary<string, IDataSource>>>(
                $"{queryDef.Title}({queryDef.Id}), " +
                $"hasLookUp:{lookUpEngineToClone != null}, " +
                $"overrides: {overrideLookUps?.Count()}, " +
                $"drafts:{showDrafts}");

	        // the query settings which apply to the whole query
	        var querySettingsLookUp = new LookUpInMetadata(ConfigKeyPipelineSettings, queryDef.Entity, _cultureResolver.SafeLanguagePriorityCodes());

            // centralizing building of the primary configuration template for each part
            var templateConfig = new LookUpEngine(lookUpEngineToClone, Log);

            if (queryDef.ParamsLookUp is LookUpInDictionary paramsLookup)
                paramsLookup.Properties[QueryConstants.ParamsShowDraftKey] = showDrafts.ToString();

            templateConfig.Add(querySettingsLookUp);        // add [pipelinesettings:...]
            templateConfig.Add(queryDef.ParamsLookUp);      // Add [params:...]
            templateConfig.AddOverride(overrideLookUps);    // add override


            // provide global settings for ShowDrafts, ATM just if showdrafts are to be used
            var itemSettingsShowDrafts = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
                {{QueryConstants.ParamsShowDraftKey, showDrafts.ToString()}};

            #endregion
			#region Load Query Entity and Query Parts

			// tell the primary-out that it has this guid, for better debugging
            var passThroughConfig = new LookUpEngine(templateConfig, Log);
            IDataSource outTarget = new PassThrough().Init(passThroughConfig);
			if (outTarget.Guid == Guid.Empty)
	            outTarget.Guid = queryDef.Entity.EntityGuid;

            #endregion

	        #region init all DataQueryParts

	        Log.Add($"add parts to pipe#{queryDef.Entity.EntityId} ");
	        var dataSources = new Dictionary<string, IDataSource>();

	        foreach (var dataQueryPart in queryDef.Parts)
	        {
	            #region Init Configuration Provider

	            var partConfig = new LookUpEngine(templateConfig, Log);
                // add / set item part configuration
	            partConfig.Add(new LookUpInMetadata(ConfigKeyPartSettings, dataQueryPart.Entity, _cultureResolver.SafeLanguagePriorityCodes()));

	            // if show-draft in overridden, add that to the settings
	            partConfig.AddOverride(new LookUpInDictionary(ConfigKeyPartSettings, itemSettingsShowDrafts));

                #endregion


                // Check type because we renamed the DLL with the parts, and sometimes the old dll-name had been saved
                var assemblyAndType = dataQueryPart.DataSourceType;

                var appIdentity = _appStates.IdentityOfApp(queryDef.AppId);
                var dataSource = _dataSourceFactory.GetDataSource(assemblyAndType, appIdentity, lookUps: partConfig);
	            dataSource.Guid = dataQueryPart.Guid;

                try
                {
                    dataSource.Label = dataQueryPart.Entity.GetBestTitle();
                } catch { /* ignore */ }

	            Log.Add($"add '{assemblyAndType}' as " +
	                    $"part#{dataQueryPart.Id}({dataQueryPart.Guid.ToString().Substring(0, 6)}...)");
	            dataSources.Add(dataQueryPart.Guid.ToString(), dataSource);
	        }
	        dataSources.Add("Out", outTarget);

	        #endregion

	        InitWirings(queryDef, dataSources);
			var result = new Tuple<IDataSource, Dictionary<string, IDataSource>>(outTarget, dataSources);
			return wrapLog($"parts:{queryDef.Parts.Count}", result);
	    }

	    /// <summary>
		/// Init Stream Wirings between Query-Parts (Bottom-Up)
		/// </summary>
		private void InitWirings(QueryDefinition queryDef, IDictionary<string, IDataSource> dataSources)
		{
			// Init
            var wirings = queryDef.Connections;
			var initializedWirings = new List<Connection>();
		    var logWrap = Log.Call($"count⋮{wirings.Count}");

			// 1. wire Out-Streams of DataSources with no In-Streams
			var dataSourcesWithNoInStreams = dataSources.Where(d => wirings.All(w => w.To != d.Key));
			ConnectOutStreams(dataSourcesWithNoInStreams, dataSources, wirings, initializedWirings);

			// 2. init DataSources with In-Streams of DataSources which are already wired
			// note: there is a bug here, because when a DS has "In" from multiple sources, then it won't always be ready to provide out...
			// repeat until all are connected
			var connectionsWereAdded = true;
		    while (connectionsWereAdded)
			{
				var dataSourcesWithInitializedInStreams = dataSources
                    .Where(d => initializedWirings.Any(w => w.To == d.Key));

				connectionsWereAdded = ConnectOutStreams(dataSourcesWithInitializedInStreams, dataSources, wirings, initializedWirings);

				//if (!connectionsCreated)
				//    break;
			}

			// 3. Test all Wirings were created
			if (wirings.Count != initializedWirings.Count)
			{
				var notInitialized = wirings.Where(w => !initializedWirings.Any(i => i.From == w.From && i.Out == w.Out && i.To == w.To && i.In == w.In));
				var error = string.Join(", ", notInitialized);
				throw new Exception("Some Stream-Wirings were not created: " + error);
			}
		    logWrap("ok");
		}

		/// <summary>
		/// Wire all Out-Wirings on specified DataSources
		/// </summary>
		private static bool ConnectOutStreams(IEnumerable<KeyValuePair<string, IDataSource>> dataSourcesToInit, IDictionary<string, IDataSource> allDataSources, IList<Connection> allWirings, List<Connection> initializedWirings)
		{
			var connectionsWereAdded = false;

			foreach (var dataSource in dataSourcesToInit)
            {
                var unassignedConnectionsForThisSource = allWirings
                    .Where(w =>
                        w.From == dataSource.Key
                        && !initializedWirings.Any(i =>
                            w.From == i.From && w.Out == i.Out && w.To == i.To && w.In == i.In));
                
                // loop all wirings from this DataSource (except already initialized)
                foreach (var wire in unassignedConnectionsForThisSource)
				{
				    try
				    {
				        var conSource = allDataSources[wire.From];
                        ((IDataTarget) allDataSources[wire.To]).Attach(wire.In, conSource, wire.Out);
                        initializedWirings.Add(wire);

                        // In the end, inform caller that we did add some connections
				        connectionsWereAdded = true;
				    }
				    catch (Exception ex)
				    {
				        throw new Exception("Trouble with connecting query from " + wire.From + ":" + wire.Out + " to " + wire.To + ":" + wire.In, ex);
				    }
				}
			}

			return connectionsWereAdded;
		}


	    public Tuple<IDataSource, Dictionary<string, IDataSource>> GetDataSourceForTesting(QueryDefinition queryDef, bool showDrafts, ILookUpEngine configuration = null)
	    {
            var wrapLog = Log.Call<Tuple<IDataSource, Dictionary<string, IDataSource>>>(
                $"a#{queryDef.AppId}, pipe:{queryDef.Entity.EntityGuid} ({queryDef.Entity.EntityId}), drafts:{showDrafts}");
            var testValueProviders = queryDef.TestParameterLookUps;
            return wrapLog(null, BuildQuery(queryDef, configuration, testValueProviders, showDrafts));
        }


	}
}
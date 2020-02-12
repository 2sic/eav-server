using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Configuration;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources.Queries
{
	/// <summary>
	/// Factory to create a Data Query
	/// </summary>
	public class QueryBuilder: HasLog
	{
	    public QueryBuilder(ILog parentLog) : base("DS.PipeFt", parentLog) {}

		/// <summary>
        /// Build a query-definition object based on the entity-ID defining the query
        /// </summary>
        /// <returns></returns>
        public QueryDefinition GetQueryDefinition(int appId, int queryEntityId)
	    {
            var wrapLog = Log.Call($"def#{queryEntityId} for a#{appId}");

	        try
            {
                var app = Apps.State.Identity(null, appId);
                var source = new DataSource(Log).GetPublishing(app);
	            var appEntities = source[Constants.DefaultStreamName].List;

	            // use findRepo, as it uses the cache, which gives the list of all items // [queryEntityId];
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


	    public IDataSource BuildQuery(QueryDefinition queryDef,
            ILookUpEngine lookUpEngineToClone,
            IEnumerable<ILookUp> overrideLookUps,
            //IDataSource outTarget = null,
            bool showDrafts)
        {
	        #region prepare shared / global value providers

	        overrideLookUps = overrideLookUps?.ToList();
	        var wrapLog = Log.Call($"{queryDef.Title}({queryDef.Id}), " +
                                            $"hasLookUp:{lookUpEngineToClone != null}, " +
                                            $"overrides: {overrideLookUps?.Count()}, " +
                                            // $"out:{outTarget != null}, " +
                                            $"drafts:{showDrafts}");

	        // the query settings which apply to the whole query
	        var querySettingsLookUp = new LookUpInMetadata(ConfigKeyPipelineSettings, queryDef.Entity);

            // centralizing building of the primary configuration template for each part
            if (lookUpEngineToClone != null)
                Log.Add(() =>
                    $"Sources in original LookUp: {lookUpEngineToClone.Sources.Count} " +
                    $"[{string.Join(",", lookUpEngineToClone.Sources.Keys)}]");
            var templateConfig = new LookUpEngine(lookUpEngineToClone, Log);

            if (queryDef.ParamsLookUp is LookUpInDictionary paramsLookup)
                paramsLookup.Properties[QueryConstants.ParamsShowDraftKey] = showDrafts.ToString();

            templateConfig.Add(querySettingsLookUp);        // add [pipelinesettings:...]
            templateConfig.Add(queryDef.ParamsLookUp);      // Add [params:...]
            templateConfig.AddOverride(overrideLookUps);    // add override


            // provide global settings for ShowDrafts, ATM just if showdrafts are to be used
            var itemSettingsShowDrafts = new Dictionary<string, string>
                {{QueryConstants.ParamsShowDraftKey, showDrafts.ToString()}};

            #endregion
			#region Load Query Entity and Query Parts

			// tell the primary-out that it has this guid, for better debugging
			IDataSource outTarget = null;
         //   if (outTarget == null)
	        //{
	            var passThroughConfig = new LookUpEngine(templateConfig, Log);
                outTarget = new PassThrough().Init(passThroughConfig);
	        //}
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
	            partConfig.Add(new LookUpInMetadata(ConfigKeyPartSettings, dataQueryPart.Entity));

	            // if show-draft in overridden, add that to the settings
	            if (itemSettingsShowDrafts != null)
	                partConfig.AddOverride(new LookUpInDictionary(ConfigKeyPartSettings, itemSettingsShowDrafts));

                #endregion


                // Check type because we renamed the DLL with the parts, and sometimes the old dll-name had been saved
                var assemblyAndType = dataQueryPart.DataSourceType;

                var appIdentity = Apps.State.Identity(null, queryDef.AppId);
                var dataSource = new DataSource(Log).GetDataSource(assemblyAndType, appIdentity, configLookUp: partConfig);
	            dataSource.Guid = dataQueryPart.Guid;

	            Log.Add($"add '{assemblyAndType}' as " +
	                    $"part#{dataQueryPart.Id}({dataQueryPart.Guid.ToString().Substring(0, 6)}...)");
	            dataSources.Add(dataQueryPart.Guid.ToString(), dataSource);
	        }
	        dataSources.Add("Out", outTarget);

	        #endregion

	        InitWirings(queryDef, dataSources);

	        wrapLog($"parts:{queryDef.Parts.Count}");
	        return outTarget;
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
		    while (true)
			{
				var dataSourcesWithInitializedInStreams = dataSources.Where(d => initializedWirings.Any(w => w.To == d.Key));

				var connectionsCreated = ConnectOutStreams(dataSourcesWithInitializedInStreams, dataSources, wirings, initializedWirings);

				if (!connectionsCreated)
				    break;
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
			var wiringsCreated = false;

			foreach (var dataSource in dataSourcesToInit)
			{
			    var unassignedConnectionsForThisSource = allWirings.Where(w =>
			        w.From == dataSource.Key &&
			        !initializedWirings.Any(i => w.From == i.From && w.Out == i.Out && w.To == i.To && w.In == i.In));
                // loop all wirings from this DataSource (except already initialized)
                foreach (var wire in unassignedConnectionsForThisSource)
				{
				    try
				    {
				        var sourceDsrc = allDataSources[wire.From];
				        var sourceStream = (sourceDsrc as IDeferredDataSource)?.DeferredOut(wire.Out) ?? sourceDsrc.Out[wire.Out]; // if the source provides deferredOut, use that
				        ((IDataTarget) allDataSources[wire.To]).In[wire.In] = sourceStream;

				        initializedWirings.Add(wire);

				        wiringsCreated = true;
				    }
				    catch (Exception ex)
				    {
				        throw new Exception("Trouble with connecting query from " + wire.From + ":" + wire.Out + " to " + wire.To + ":" + wire.In, ex);
				    }
				}
			}

			return wiringsCreated;
		}


	    public IDataSource GetDataSourceForTesting(QueryDefinition queryDef, bool showDrafts, ILookUpEngine configuration = null)
	    {
            var wrapLog = Log.Call<IDataSource>($"a#{queryDef.AppId}, pipe:{queryDef.Entity.EntityGuid} ({queryDef.Entity.EntityId}), drafts:{showDrafts}");
            var testValueProviders = queryDef.TestParameterLookUps;
            return wrapLog(null,
                BuildQuery(queryDef, configuration, testValueProviders, /*outTarget: null,*/ showDrafts));
        }


	}
}
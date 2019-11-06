using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ToSic.Eav.Data.Query;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.ValueProvider;
using ToSic.Eav.ValueProviders;

namespace ToSic.Eav.DataSources.Pipeline
{
	/// <summary>
	/// Factory to create a Data Query
	/// </summary>
	public class QueryFactory: HasLog
	{
	    public QueryFactory(ILog parentLog) : base("DS.PipeFt", parentLog) {}

	    /// <summary>
	    /// Creates a Query DataSource from a QueryEntity for specified App
	    /// </summary>
	    /// <param name="appId">AppId to use</param>
	    /// <param name="query">The entity describing the this data source part in the query</param>
	    /// <param name="valueCollection">ConfigurationProvider Provider for configurable DataSources</param>
	    /// <param name="outSource">DataSource to attach the Out-Streams</param>
	    /// <param name="showDrafts"></param>
	    /// <returns>A single DataSource Out with wirings and configurations loaded, ready to use</returns>
	    public IDataSource GetAsDataSource(int appId, IEntity query, IValueCollectionProvider valueCollection, IDataSource outSource = null, bool showDrafts = false)
	    {
		    Log.Add($"build pipe#{query.EntityId} for a#{appId}, draft:{showDrafts}");
		    //var configurationPropertyAccesses = valueCollection.Sources.Select(s => s.Value);
            var qdef = new QueryDefinition(query, appId);
	        return GetAsDataSource(qdef,  valueCollection,/*configurationPropertyAccesses*/ null, outSource, showDrafts);
		}


        /// <summary>
        /// Build a query-definition object based on the entity-ID defining the query
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="queryEntityId"></param>
        /// <returns></returns>
	    public QueryDefinition GetQueryDefinition(int appId, int queryEntityId)
	    {
	        Log.Add($"get query def#{queryEntityId} for a#{appId}");

	        try
	        {
                var source = DataSource.GetInitialDataSource(appId: appId, parentLog: Log);
	            var appEntities = source[Constants.DefaultStreamName].List;

	            // use findRepo, as it uses the cache, which gives the list of all items // [queryEntityId];
	            var dataQuery = appEntities.FindRepoId(queryEntityId);
	            return new QueryDefinition(dataQuery, appId);
	        }
	        catch (KeyNotFoundException)
	        {
	            throw new Exception("QueryEntity not found with ID " + queryEntityId + " on AppId " + appId);
	        }

	    }

        public const string ConfigKeyPartSettings = "settings";
	    public const string ConfigKeyPipelineSettings = "pipelinesettings";

	    private IDataSource GetAsDataSource(QueryDefinition qdef,
            IValueCollectionProvider providerToClone,
            IEnumerable<IValueProvider> propertyProviders = null,
            IDataSource outSource = null,
            bool showDrafts = false)
        {
	        #region prepare shared / global value providers

	        propertyProviders = propertyProviders?.ToList();
	        var wrapLog = Log.Call("GetAsDataSource", $"{qdef.Header.EntityId}, " +
	                                                  $"hasProv:{providerToClone != null}, " +
	                                                  $"{propertyProviders?.Count()}, " +
	                                                  $"out:{outSource != null}, " +
	                                                  $"drafts:{showDrafts}");

	        // the query settings which apply to the whole query
            // todo 2017-12-05 2dm - this is probably where I will apply parameters, I think it's not used yet!
	        var querySettingsProvider = new AssignedEntityValueProvider(ConfigKeyPipelineSettings, qdef.Header);

            // 2018-09-30 2dm - centralizing building of the primary configuration template for each part
            if (providerToClone != null)
                Log.Add(() =>
                    $"Sources in original provider: {providerToClone.Sources.Count} " +
                    $"[{string.Join(",", providerToClone.Sources.Keys)}]");
            var templateConfig = new ValueCollectionProvider(providerToClone);
            templateConfig.Add(querySettingsProvider);  
            templateConfig.AddOverride(propertyProviders);

	        var itemSettingsShowDrafts = showDrafts
	            ? new Dictionary<string, string> {{"ShowDrafts", true.ToString()}}
	            : null;

            // global settings, ATM just if showdrafts are to be used

            #endregion
            #region Load Query Entity and Query Parts

            // tell the primary-out that it has this guid, for better debugging
            if (outSource == null)
	        {
	            var passThroughConfig = new ValueCollectionProvider(templateConfig);
	            outSource = new PassThrough {ConfigurationProvider = passThroughConfig};
	        }
            if (outSource.DataSourceGuid == Guid.Empty)
	            outSource.DataSourceGuid = qdef.Header.EntityGuid;

	        #endregion

	        #region init all DataQueryParts

	        Log.Add($"add parts to pipe#{qdef.Header.EntityId} ");
	        var dataSources = new Dictionary<string, IDataSource>();

	        foreach (var dataQueryPart in qdef.Parts)
	        {
	            #region Init Configuration Provider

	            var partConfig = new ValueCollectionProvider(templateConfig);
                // add / set item part configuration
	            partConfig.Add(new AssignedEntityValueProvider(ConfigKeyPartSettings, dataQueryPart));

	            // if show-draft in overridden, add that to the settings
	            if (itemSettingsShowDrafts != null)
	                partConfig.AddOverride(new StaticValueProvider(ConfigKeyPartSettings, itemSettingsShowDrafts));

                #endregion


                // Check type because we renamed the DLL with the parts, and sometimes the old dll-name had been saved
                var assemblyAndType = dataQueryPart[QueryConstants.PartAssemblyAndType][0].ToString();
	            assemblyAndType = RewriteOldAssemblyNames(assemblyAndType);

	            var dataSource = DataSource.GetDataSource(assemblyAndType, null, qdef.AppId,
	                valueCollectionProvider: partConfig, parentLog: Log);
	            dataSource.DataSourceGuid = dataQueryPart.EntityGuid;

	            Log.Add($"add '{assemblyAndType}' as " +
	                    $"part#{dataQueryPart.EntityId}({dataQueryPart.EntityGuid.ToString().Substring(0, 6)}...)");
	            dataSources.Add(dataQueryPart.EntityGuid.ToString(), dataSource);
	        }
	        dataSources.Add("Out", outSource);

	        #endregion

	        InitWirings(qdef.Header, dataSources);

	        wrapLog($"parts:{qdef.Parts.Count}");
	        return outSource;
	    }

	    /// <summary>
	    /// Check if a Query part has an old assembly name, and if yes, correct it to the new name
	    /// </summary>
	    /// <param name="assemblyAndType"></param>
	    /// <returns></returns>
	    public static string RewriteOldAssemblyNames(string assemblyAndType)
	        => assemblyAndType.EndsWith(Constants.V3To4DataSourceDllOld)
	            ? assemblyAndType.Replace(Constants.V3To4DataSourceDllOld, Constants.V3To4DataSourceDllNew)
	            : assemblyAndType;

	    /// <summary>
		/// Init Stream Wirings between Query-Parts (Buttom-Up)
		/// </summary>
		private void InitWirings(IEntity dataQuery, IDictionary<string, IDataSource> dataSources)
		{
			// Init
			var wirings = QueryWiring.Deserialize((string)dataQuery[Constants.QueryStreamWiringAttributeName][0]).ToList();
			var initializedWirings = new List<WireInfo>();
		    var logWrap = Log.Call("InitWirings", $"count⋮{wirings.Count}");

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
		private static bool ConnectOutStreams(IEnumerable<KeyValuePair<string, IDataSource>> dataSourcesToInit, IDictionary<string, IDataSource> allDataSources, List<WireInfo> allWirings, List<WireInfo> initializedWirings)
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


	    public IDataSource GetDataSourceForTesting(QueryDefinition qdef, bool showDrafts, IValueCollectionProvider configuration = null)
	    {
	        Log.Add($"construct test query a#{qdef.AppId}, pipe:{qdef.Header.EntityGuid} ({qdef.Header.EntityId}), drafts:{showDrafts}");

            var testValueProviders = GenerateTestValueProviders(qdef).ToList();
	        return GetAsDataSource(qdef, configuration, testValueProviders, showDrafts: showDrafts);
	    }

	    private const string FieldTestParams = "TestParameters";
        /// <summary>
        /// Retrieve test values to test a specific query. 
        /// The specs are found in the query definition, but the must be converted to a source
        /// They are in the format [source:key]=value
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IValueProvider> GenerateTestValueProviders(QueryDefinition qdef)
        {
            var wrapLog = Log.Call("GenerateTestValueProviders", $"{qdef.Header.EntityId}");
            // Parse Test-Parameters in Format [Token:Property]=Value
            var testParameters = ((IAttribute<string>) qdef.Header[FieldTestParams]).TypedContents;
            if (testParameters == null)
                return null;
            // extract the lines which look like [source:property]=value
            const string keyToken = "Token", 
                keyProperty = "Property", 
                keyValue = "Value";
            var paramMatches = Regex.Matches(testParameters, @"(?:\[(?<Token>\w+):(?<Property>\w+)\])=(?<Value>[^\r\n]*)");

            // Create a list of static Property Accessors
            var result = new List<IValueProvider>();
            foreach (Match testParam in paramMatches)
            {
                var token = testParam.Groups[keyToken].Value.ToLower();

                // Ensure a PropertyAccess exists
                if (!(result.FirstOrDefault(i => i.Name == token) is StaticValueProvider propertyAccess))
                {
                    propertyAccess = new StaticValueProvider(token);
                    result.Add(propertyAccess);
                }

                // Add the static value
                propertyAccess.Properties.Add(testParam.Groups[keyProperty].Value, testParam.Groups[keyValue].Value);
            }
            wrapLog("ok");
            return result;
        }

	}
}
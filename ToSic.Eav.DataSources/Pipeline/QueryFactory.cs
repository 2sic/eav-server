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

namespace ToSic.Eav.DataSources.Pipeline
{
	/// <summary>
	/// Factory to create a Data Query
	/// </summary>
	public class QueryFactory: HasLog
	{
	    public QueryFactory(Log parentLog) : base("DS.PipeFt", parentLog) {}

	    /// <summary>
	    /// Creates a DataSource from a QueryEntity for specified Zone and App
	    /// </summary>
	    /// <param name="appId">AppId to use</param>
	    /// <param name="valueCollection">ConfigurationProvider Provider for configurable DataSources</param>
	    /// <param name="outSource">DataSource to attach the Out-Streams</param>
	    /// <param name="showDrafts"></param>
	    /// <returns>A single DataSource Out with wirings and configurations loaded, ready to use</returns>
	    public IDataSource GetDataSource(int appId, IEntity query, ValueCollectionProvider valueCollection, IDataSource outSource = null, bool showDrafts = false)
	    {
	        var queryEntityId = query.EntityId;
		    Log.Add($"build pipe#{queryEntityId} for a#{appId}, draft:{showDrafts}");
		    var configurationPropertyAccesses = valueCollection.Sources.Select(s => s.Value);
            var qdef = new QueryDefinition(query, appId); // GetQueryDefinition(appId, queryEntityId);
            return GetDataSource(qdef, configurationPropertyAccesses, outSource, showDrafts);
		}

	    private QueryDefinition GetQueryDefinition(int appId, int queryEntityId)
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

	    public IDataSource GetDataSource(QueryDefinition qdef, 
            IEnumerable<IValueProvider> propertyProviders,
	        IDataSource outSource = null, 
            bool showDrafts = false)
	    {


	        #region prepare shared / global value providers

	        propertyProviders = propertyProviders?.ToList();
	        // the query settings which apply to the whole query
            // todo 2017-12-05 2dm - this is probably where I will apply parameters, I think it's not used yet!
	        var querySettingsProvider = new AssignedEntityValueProvider("pipelinesettings", qdef.Header);


            // global settings, ATM just if showdrafts are to be used
            const string itemSettings = "settings";

	        #endregion
	        #region Load Query Entity and Query Parts

	        // tell the primary-out that it has this guid, for better debugging
	        if (outSource == null)
	        {
	            var ptValues = new ValueCollectionProvider();
                ptValues.Add(querySettingsProvider);
                ptValues.AddOverride(propertyProviders);

	            var pass = new PassThrough {ConfigurationProvider = ptValues};
	            outSource = pass;
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

	            var configurationProvider = new ValueCollectionProvider();
	            configurationProvider.Add(new AssignedEntityValueProvider(itemSettings, dataQueryPart));

	            // if show-draft in overridden, add that to the settings
	            if (showDrafts)
	                configurationProvider.AddOverride(new StaticValueProvider(itemSettings,
	                    new Dictionary<string, string>
	                    {
	                        {"ShowDrafts", true.ToString()}
	                    }));

                configurationProvider.Add(querySettingsProvider);

	            // ReSharper disable once PossibleMultipleEnumeration
                configurationProvider.AddOverride(propertyProviders);

	            #endregion


	            // This is new in 2015-10-38 - check type because we renamed the DLL with the parts, and sometimes the old dll-name had been saved
	            var assemblyAndType = dataQueryPart[QueryConstants.PartAssemblyAndType][0].ToString();
	            assemblyAndType = RewriteOldAssemblyNames(assemblyAndType);

	            var dataSource = DataSource.GetDataSource(assemblyAndType, null, qdef.AppId,
	                valueCollectionProvider: configurationProvider, parentLog: Log);
	            dataSource.DataSourceGuid = dataQueryPart.EntityGuid;

	            Log.Add($"add '{assemblyAndType}' as " +
	                    $"part#{dataQueryPart.EntityId}({dataQueryPart.EntityGuid.ToString().Substring(0, 6)}...)");
	            dataSources.Add(dataQueryPart.EntityGuid.ToString(), dataSource);
	        }
	        dataSources.Add("Out", outSource);

	        #endregion

	        InitWirings(qdef.Header, dataSources);

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
		    Log.Add($"init wirings⋮{wirings.Count}");

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


        /// <summary>
        /// Build a data-source using test-values
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="queryId"></param>
        /// <param name="showDrafts"></param>
        /// <returns></returns>
	    public IDataSource GetDataSourceForTesting(int appId, int queryId, bool showDrafts) 
            => GetDataSourceForTesting(GetQueryDefinition(appId, queryId), showDrafts);

	    public IDataSource GetDataSourceForTesting(QueryDefinition qdef, bool showDrafts)
	    {
	        Log.Add($"construct test query a#{qdef.AppId}, pipe:{qdef.Header.EntityGuid} ({qdef.Header.EntityId}), drafts:{showDrafts}");

            var testValueProviders = GetTestValueProviders(qdef).ToList();
	        return GetDataSource(qdef, testValueProviders, showDrafts: showDrafts);
	    }

	    private const string FieldTestParams = "TestParameters";
        /// <summary>
        /// Retrieve test values to test a specific query. 
        /// The specs are found in the query definition, but the must be converted to a source
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IValueProvider> GetTestValueProviders(QueryDefinition qdef)
        {
            // Parse Test-Parameters in Format [Token:Property]=Value
            var testParameters = ((IAttribute<string>) qdef.Header[FieldTestParams]).TypedContents;
            if (testParameters == null)
                return null;
            // extract the lines which look like [source:property]=value
            var paramMatches = Regex.Matches(testParameters, @"(?:\[(?<Token>\w+):(?<Property>\w+)\])=(?<Value>[^\r\n]*)");

            // Create a list of static Property Accessors
            var result = new List<IValueProvider>();
            foreach (Match testParam in paramMatches)
            {
                var token = testParam.Groups["Token"].Value.ToLower();

                // Ensure a PropertyAccess exists
                if (!(result.FirstOrDefault(i => i.Name == token) is StaticValueProvider propertyAccess))
                {
                    propertyAccess = new StaticValueProvider(token);
                    result.Add(propertyAccess);
                }

                // Add the static value
                propertyAccess.Properties.Add(testParam.Groups["Property"].Value, testParam.Groups["Value"].Value);
            }

            return result;
        }
	}
}
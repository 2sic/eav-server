using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Factory to create a Data Pipeline
	/// </summary>
	public class DataPipelineFactory
	{
		/// <summary>
		/// Creates a DataSource from a PipelineEntity for specified Zone and App
		/// </summary>
		/// <param name="appId">AppId to use</param>
		/// <param name="pipelineEntityId">EntityId of the Entity describing the Pipeline</param>
		/// <param name="valueCollection">ConfigurationProvider Provider for configurable DataSources</param>
		/// <param name="outSource">DataSource to attach the Out-Streams</param>
		/// <returns>A single DataSource Out with wirings and configurations loaded, ready to use</returns>
		public static IDataSource GetDataSource(int appId, int pipelineEntityId, ValueCollectionProvider valueCollection, IDataSource outSource = null)
		{
			return GetDataSource(appId, pipelineEntityId, valueCollection.Sources.Select(s => s.Value), outSource);
		}

		/// <summary>
		/// Creates a DataSource from a PipelineEntity for specified Zone and App
		/// </summary>
		/// <param name="appId">AppId to use</param>
		/// <param name="pipelineEntityId">EntityId of the Entity describing the Pipeline</param>
		/// <param name="configurationPropertyAccesses">Property Providers for configurable DataSources</param>
		/// <param name="outSource">DataSource to attach the Out-Streams</param>
		/// <returns>A single DataSource Out with wirings and configurations loaded, ready to use</returns>
		public static IDataSource GetDataSource(int appId, int pipelineEntityId, IEnumerable<IValueProvider> configurationPropertyAccesses, IDataSource outSource = null)
		{
            if(outSource == null)
                outSource = new PassThrough();
			#region Load Pipeline Entity and Pipeline Parts
			var source = DataSource.GetInitialDataSource(appId: appId);
			var metaDataSource = DataSource.GetMetaDataSource(source.ZoneId, source.AppId);	// ToDo: Validate change/extension with zoneId and appId Parameter

			var appEntities = source[Constants.DefaultStreamName].List;
			IEntity dataPipeline;
			try
			{
				dataPipeline = appEntities[pipelineEntityId];
			}
			catch (KeyNotFoundException)
			{
				throw new Exception("PipelineEntity not found with ID " + pipelineEntityId + " on AppId " + appId);
			}
			var dataPipelineParts = metaDataSource.GetAssignedEntities(Constants.AssignmentObjectTypeEntity, dataPipeline.EntityGuid, Constants.DataPipelinePartStaticName);
			#endregion

			var pipelineSettingsProvider = new AssignedEntityValueProvider("pipelinesettings", dataPipeline.EntityGuid, metaDataSource);
			#region init all DataPipelineParts
			var dataSources = new Dictionary<string, IDataSource>();
		    foreach (var dataPipelinePart in dataPipelineParts)
		    {
		        #region Init Configuration Provider

		        var configurationProvider = new ValueCollectionProvider();
		        var settingsPropertySource = new AssignedEntityValueProvider("settings", dataPipelinePart.EntityGuid,
		            metaDataSource);
		        configurationProvider.Sources.Add(settingsPropertySource.Name, settingsPropertySource);
		        configurationProvider.Sources.Add(pipelineSettingsProvider.Name, pipelineSettingsProvider);

		        // attach all propertyProviders
		        if (configurationPropertyAccesses != null)
		            foreach (var propertyProvider in configurationPropertyAccesses)
		            {
		                if (propertyProvider.Name == null)
		                    throw new NullReferenceException("PropertyProvider must have a Name");
		                configurationProvider.Sources.Add(propertyProvider.Name, propertyProvider);
		            }

		        #endregion


		        // This is new in 2015-10-38 - check type because we renamed the DLL with the parts, and sometimes the old dll-name had been saved
		        var assemblyAndType = dataPipelinePart["PartAssemblyAndType"][0].ToString();
		        if (assemblyAndType.EndsWith(Constants.V3To4DataSourceDllOld))
		            assemblyAndType = assemblyAndType.Replace(Constants.V3To4DataSourceDllOld, Constants.V3To4DataSourceDllNew);

		        var dataSource = DataSource.GetDataSource(assemblyAndType,
		            source.ZoneId, source.AppId, valueCollectionProvider: configurationProvider);
		        dataSource.DataSourceGuid = dataPipelinePart.EntityGuid;
		        //ConfigurationProvider.configList = dataSource.Configuration;

		        dataSources.Add(dataPipelinePart.EntityGuid.ToString(), dataSource);

		    }
		    dataSources.Add("Out", outSource);
			#endregion

			InitWirings(dataPipeline, dataSources);

			return outSource;
		}

		/// <summary>
		/// Init Stream Wirings between Pipeline-Parts (Buttom-Up)
		/// </summary>
		private static void InitWirings(IEntity dataPipeline, IDictionary<string, IDataSource> dataSources)
		{
			// Init
			var wirings = DataPipelineWiring.Deserialize((string)dataPipeline[Constants.DataPipelineStreamWiringStaticName][0]);
			var initializedWirings = new List<WireInfo>();

			// 1. wire Out-Streams of DataSources with no In-Streams
			var dataSourcesWithNoInStreams = dataSources.Where(d => wirings.All(w => w.To != d.Key));
			ConnectOutStreams(dataSourcesWithNoInStreams, dataSources, wirings, initializedWirings);

			// 2. init DataSources with In-Streams of DataSources which are already wired
			// repeat until all are connected
			while (true)
			{
				var dataSourcesWithInitializedInStreams = dataSources.Where(d => initializedWirings.Any(w => w.To == d.Key));
				var connectionsCreated = ConnectOutStreams(dataSourcesWithInitializedInStreams, dataSources, wirings, initializedWirings);

				if (!connectionsCreated)
					break;
			}

			// 3. Test all Wirings were created
			if (wirings.Count() != initializedWirings.Count)
			{
				var notInitialized = wirings.Where(w => !initializedWirings.Any(i => i.From == w.From && i.Out == w.Out && i.To == w.To && i.In == w.In));
				var error = string.Join(", ", notInitialized);
				throw new Exception("Some Stream-Wirings were not created: " + error);
			}
		}

		/// <summary>
		/// Wire all Out-Wirings on specified DataSources
		/// </summary>
		private static bool ConnectOutStreams(IEnumerable<KeyValuePair<string, IDataSource>> dataSourcesToInit, IDictionary<string, IDataSource> allDataSources, IEnumerable<WireInfo> allWirings, List<WireInfo> initializedWirings)
		{
			var wiringsCreated = false;

			foreach (var dataSource in dataSourcesToInit)
			{
				// loop all wirings from this DataSource (except already initialized)
				foreach (var wire in allWirings.Where(w => w.From == dataSource.Key && !initializedWirings.Any(i => w.From == i.From && w.Out == i.Out && w.To == i.To && w.In == i.In)))
				{
					var sourceDsrc = allDataSources[wire.From];
					((IDataTarget)allDataSources[wire.To]).In[wire.In] = sourceDsrc.Out[wire.Out];

					initializedWirings.Add(wire);

					wiringsCreated = true;
				}
			}

			return wiringsCreated;
		}

		/// <summary>
		/// Find a DataSource of a specific Type in a DataPipeline
		/// </summary>
		/// <typeparam name="T">Type of the DataSource to find</typeparam>
		/// <param name="rootDataSource">DataSource to look for In-Connections</param>
		/// <returns>DataSource of specified Type or null</returns>
		public static T FindDataSource<T>(IDataTarget rootDataSource) where T : IDataSource
		{
			foreach (var stream in rootDataSource.In)
			{
				// If type matches, return this DataSource
				if (stream.Value.Source.GetType() == typeof(T))
					return (T)stream.Value.Source;

				// Find recursive in In-Streams of this DataSource (if any)
				var dataTarget = stream.Value.Source as IDataTarget;
				if (dataTarget != null)
					return FindDataSource<T>(dataTarget);
			}

			return default(T);
		}

        /// <summary>
        /// Retrieve test values to test a specific pipeline. 
        /// The specs are found in the pipeline definition, but the must be converted to a source
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pipelineEntityId"></param>
        /// <returns></returns>
        public static IEnumerable<IValueProvider> GetTestValueProviders(int appId, int pipelineEntityId)
        {
            // Get the Entity describing the Pipeline
            var source = DataSource.GetInitialDataSource(appId: appId);
            var pipelineEntity = DataPipeline.GetPipelineEntity(pipelineEntityId, source);

            // Parse Test-Parameters in Format [Token:Property]=Value
            var testParameters = ((IAttribute<string>)pipelineEntity["TestParameters"]).TypedContents;
            if (testParameters == null)
                return null;
            // todo: dangerous: seems like another token-replace mechanism!
            var paramMatches = Regex.Matches(testParameters, @"(?:\[(?<Token>\w+):(?<Property>\w+)\])=(?<Value>[^\r\n]*)");

            // Create a list of static Property Accessors
            var result = new List<IValueProvider>();
            foreach (Match testParam in paramMatches)
            {
                var token = testParam.Groups["Token"].Value.ToLower();

                // Ensure a PropertyAccess exists
                var propertyAccess = result.FirstOrDefault(i => i.Name == token) as StaticValueProvider;
                if (propertyAccess == null)
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
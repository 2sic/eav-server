using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.LookUp;
using ToSic.Lib.Logging;
using ToSic.Eav.LookUp;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using static System.StringComparer;

namespace ToSic.Eav.DataSources.Queries
{
	/// <summary>
	/// Factory to create a Data Query
	/// </summary>
	public class QueryBuilder: ServiceBase
	{
        private readonly IContextResolverUserPermissions _userPermissions;
        private readonly Generator<PassThrough> _passThrough;
        private readonly DataSourceFactory _dataSourceFactory;
        private readonly IZoneCultureResolver _cultureResolver;
        private readonly IAppStates _appStates;

        #region Dependency Injection

        /// <summary>
		/// DI Constructor
		/// </summary>
		/// <remarks>
		/// Never call this constructor from your code, as it re-configures the DataSourceFactory it gets
		/// </remarks>
        public QueryBuilder(
            DataSourceFactory dataSourceFactory, 
            IZoneCultureResolver cultureResolver,
			Generator<PassThrough> passThrough,
            IAppStates appStates,
            IContextResolverUserPermissions userPermissions
            ) : base("DS.PipeFt")
        {
            ConnectServices(
                _cultureResolver = cultureResolver,
                _appStates = appStates,
                _dataSourceFactory = dataSourceFactory,
                _dataSourceFactory,
                _passThrough = passThrough,
                _userPermissions = userPermissions
            );
        }

        

        #endregion

        /// <summary>
        /// Build a query-definition object based on the entity-ID defining the query
        /// </summary>
        /// <returns></returns>
        public QueryDefinition GetQueryDefinition(int appId, int queryEntityId) => Log.Func($"def#{queryEntityId} for a#{appId}", () =>
        {
            try
            {
                var app = _appStates.IdentityOfApp(appId);
                var source = _dataSourceFactory.GetPublishing(appIdentity: app);
                var appEntities = source.List;

                // use findRepo, as it uses the cache, which gives the list of all items
                var dataQuery = appEntities.FindRepoId(queryEntityId);
                var result = new QueryDefinition(dataQuery, appId, Log);
                return (result);
            }
            catch (KeyNotFoundException)
            {
                throw new Exception("QueryEntity not found with ID " + queryEntityId + " on AppId " + appId);
            }
        });

        // 2023-02-10 2dm removed/changed to MyConfig because it would conflict with the new Settings lookup
        // https://github.com/2sic/2sxc/issues/3001
        // Remove this comment 2023 Q2
        //public const string ConfigKeyPartSettings = "settings";

        // 2023-02-10 2dm - removing the #PipelineSettings
        //public const string ConfigKeyPipelineSettings = "pipelinesettings";


	    public (IDataSource Main, Dictionary<string, IDataSource> DataSources) BuildQuery(QueryDefinition queryDef,
            ILookUpEngine lookUpEngineToClone,
            List<ILookUp> overrideLookUps,
            bool? showDrafts = null
        ) => Log.Func($"{queryDef.Title}({queryDef.Id}), hasLookUp:{lookUpEngineToClone != null}, overrides: {overrideLookUps?.Count}, drafts:{showDrafts}", l =>
        {
	        #region prepare shared / global value providers
            
			// 2023-02-10 2dm - removing the #PipelineSettings - clean up 2023 Q2
			// I believe this was an old feature which was never used, and super-seeded by Params
	        // the query settings which apply to the whole query
	        //var querySettingsLookUp = new LookUpInMetadata(ConfigKeyPipelineSettings, queryDef.Entity, _cultureResolver.SafeLanguagePriorityCodes());

            // centralizing building of the primary configuration template for each part
            var templateConfig = new LookUpEngine(lookUpEngineToClone, Log);

            var showDraftsFinal = showDrafts ?? _userPermissions.UserPermissions().UserMayEdit;
            if (queryDef.ParamsLookUp is LookUpInDictionary paramsLookup)
                paramsLookup.Properties[QueryConstants.ParamsShowDraftKey] = showDraftsFinal.ToString();

            // 2023-02-10 2dm - removing the #PipelineSettings
            //templateConfig.Add(querySettingsLookUp);        // add [pipelinesettings:...]
            templateConfig.Add(queryDef.ParamsLookUp);      // Add [params:...]
            templateConfig.AddOverride(overrideLookUps);    // add override


            // provide global settings for ShowDrafts, ATM just if showdrafts are to be used
            var itemSettingsShowDrafts = new Dictionary<string, string>(InvariantCultureIgnoreCase)
                {{QueryConstants.ParamsShowDraftKey, showDrafts.ToString()}};

            #endregion

			#region Load Query Entity and Query Parts

			// tell the primary-out that it has this guid, for better debugging
            var passThroughConfig = new LookUpEngine(templateConfig, Log);
            IDataSource outTarget = _passThrough.New().Init(passThroughConfig);
			if (outTarget.Guid == Guid.Empty)
	            outTarget.Guid = queryDef.Entity.EntityGuid;

            #endregion

            #region Load Parameters needed for all parts

            var appIdentity = _appStates.IdentityOfApp(queryDef.AppId);
            var dimensions = _cultureResolver.SafeLanguagePriorityCodes();

            #endregion

            #region init all DataQueryParts

            l.A($"add parts to pipe#{queryDef.Entity.EntityId} ");
	        var dataSources = new Dictionary<string, IDataSource>();
            var parts = queryDef.Parts;
            l.A($"parts:{parts.Count}");
			
            // More logging in unexpected case that we do not have parts.
            if (parts.Count == 0) l.A($"qd.Entity.Metadata:{queryDef.Entity.Metadata.Count()}");

            foreach (var dataQueryPart in parts)
	        {
	            #region Init Configuration Provider

	            var partConfig = new LookUpEngine(templateConfig, Log);
                // add / set item part configuration
	            partConfig.Add(new LookUpInQueryMetadata(DataSource.MyConfiguration, dataQueryPart.Entity, dimensions));

	            // if show-draft in overridden, add that to the settings
	            partConfig.AddOverride(new LookUpInDictionary(DataSource.MyConfiguration, itemSettingsShowDrafts));

                #endregion

                // Check type because we renamed the DLL with the parts, and sometimes the old dll-name had been saved
                var dsType = dataQueryPart.DataSourceType;

                var dataSource = _dataSourceFactory.Create(dsType, appIdentity, upstream: null, lookUps: partConfig);
	            dataSource.Guid = dataQueryPart.Guid;

                try
                {
                    dataSource.Label = dataQueryPart.Entity.GetBestTitle();
                } catch { /* ignore */ }

                var partGuidStr = dataQueryPart.Guid.ToString();

                l.A($"add '{dsType.FullName}' as part#{dataQueryPart.Id}({partGuidStr.Substring(0, 6)}...)");
	            dataSources.Add(partGuidStr, dataSource);
	        }
	        dataSources.Add("Out", outTarget);

	        #endregion

	        InitWirings(queryDef, dataSources);
			return ((outTarget, dataSources), $"parts:{parts.Count}");
	    });

	    /// <summary>
		/// Init Stream Wirings between Query-Parts (Bottom-Up)
		/// </summary>
		private void InitWirings(QueryDefinition queryDef, IDictionary<string, IDataSource> dataSources
        ) => Log.Do($"count⋮{queryDef.Connections?.Count}",l => 
		{
			// Init
            var wirings = queryDef.Connections;
			var initializedWirings = new List<Connection>();

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
            }

			// 3. Test all Wirings were created
			if (wirings.Count != initializedWirings.Count)
			{
				var notInitialized = wirings.Where(w => !initializedWirings.Any(i => i.From == w.From && i.Out == w.Out && i.To == w.To && i.In == w.In));
				var error = string.Join(", ", notInitialized);
				var exception = new Exception("Some Stream-Wirings were not created: " + error);
				l.Ex(exception);
				throw exception;
			}
		});

		/// <summary>
		/// Wire all Out-Wirings on specified DataSources
		/// </summary>
		private static bool ConnectOutStreams(
            IEnumerable<KeyValuePair<string, IDataSource>> dataSourcesToInit,
            IDictionary<string, IDataSource> allDataSources,
            IList<Connection> allWirings,
            List<Connection> initializedWirings)
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


        public (IDataSource Main, Dictionary<string, IDataSource> DataSources) GetDataSourceForTesting(
            QueryDefinition queryDef,
            bool showDrafts,
            ILookUpEngine lookUps = null
        ) => Log.Func($"a#{queryDef.AppId}, pipe:{queryDef.Entity.EntityGuid} ({queryDef.Entity.EntityId}), drafts:{showDrafts}", () =>
        {
            var testValueProviders = queryDef.TestParameterLookUps;
            return BuildQuery(queryDef, lookUps, testValueProviders, showDrafts);
        });


    }
}
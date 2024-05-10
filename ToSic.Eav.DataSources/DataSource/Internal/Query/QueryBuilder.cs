﻿using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.LookUp;
using ToSic.Eav.LookUp;
using ToSic.Eav.Services;

namespace ToSic.Eav.DataSource.Internal.Query;

/// <summary>
/// Factory to create a Data Query
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class QueryBuilder(
    IDataSourcesService dataSourceFactory,
    IZoneCultureResolver cultureResolver,
    Generator<PassThrough> passThrough,
    IAppStates appStates,
    IContextResolverUserPermissions userPermissions,
    QueryDefinitionBuilder queryDefinitionBuilder)
    : ServiceBase("DS.PipeFt",
        connect: [cultureResolver, appStates, dataSourceFactory, passThrough, userPermissions, queryDefinitionBuilder])
{

    public QueryDefinition Create(IEntity entity, int appId) => queryDefinitionBuilder.Create(entity, appId);

    /// <summary>
    /// Build a query-definition object based on the entity-ID defining the query
    /// </summary>
    /// <returns></returns>
    public QueryDefinition GetQueryDefinition(int appId, int queryEntityId) => Log.Func($"def#{queryEntityId} for a#{appId}", () =>
    {
        try
        {
            var app = appStates.IdentityOfApp(appId);
            var source = dataSourceFactory.CreateDefault(new DataSourceOptions(appIdentity: app));
            var appEntities = source.List;

            // use findRepo, as it uses the cache, which gives the list of all items
            var dataQuery = appEntities.FindRepoId(queryEntityId);
            var result = Create(dataQuery, appId);
            return (result);
        }
        catch (KeyNotFoundException)
        {
            throw new("QueryEntity not found with ID " + queryEntityId + " on AppId " + appId);
        }
    });


    public QueryResult BuildQuery(QueryDefinition queryDef,
        ILookUpEngine lookUpEngineToClone,
        List<ILookUp> overrideLookUps) 
    {
        var l = Log.Fn<QueryResult>($"{queryDef.Title}({queryDef.Id}), hasLookUp:{lookUpEngineToClone != null}, overrides: {overrideLookUps?.Count}");
        #region prepare shared / global value providers
            
        var showDrafts = userPermissions.UserPermissions().IsContentAdmin;
        if (queryDef.ParamsLookUp is LookUpInDictionary paramsLookup)
            paramsLookup.Properties[QueryConstants.ParamsShowDraftsKey] = showDrafts.ToString();

        // centralizing building of the primary configuration template for each part
        var baseLookUp = new LookUpEngine(lookUpEngineToClone, Log, sources: [queryDef.ParamsLookUp], overrides: overrideLookUps);

        //baseLookUp.Add(queryDef.ParamsLookUp);      // Add [params:...]
        //baseLookUp.AddOverride(overrideLookUps);    // add override

        // 2023-03-13 2dm - #removedQueryPartShowDrafts - it's available on [Params:ShowDrafts] and I don't think every source needs it too
        // provide global settings for ShowDrafts, ATM just if showdrafts are to be used
        //var itemSettingsShowDrafts = new Dictionary<string, string>(InvariantCultureIgnoreCase)
        //    {{QueryConstants.ParamsShowDraftKey, showDrafts.ToString()}};

        #endregion

        #region Load Query Entity and Query Parts

        // tell the primary-out that it has this guid, for better debugging
        var passThroughLookUp = new LookUpEngine(baseLookUp, Log);
        IDataSource outTarget = passThrough.New().Init(passThroughLookUp);
        if (outTarget.Guid == Guid.Empty)
            outTarget.AddDebugInfo(queryDef.Entity.EntityGuid, null);

        #endregion

        #region Load Parameters needed for all parts

        var appIdentity = appStates.IdentityOfApp(queryDef.AppId);
        var dimensions = cultureResolver.SafeLanguagePriorityCodes();

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

            var querySpecsLookUp = new LookUpInQueryPartMetadata(DataSourceConstants.MyConfigurationSourceName, dataQueryPart.Entity, dimensions);
            var partEngine = new LookUpEngine(baseLookUp, Log, sources: [querySpecsLookUp]);
            // add / set item part configuration
            //partLookUp.Add(querySpecsLookUp);
            var partConfig = new DataSourceOptions(lookUp: partEngine, appIdentity: appIdentity);

            // 2023-03-13 2dm - #removedQueryPartShowDrafts
            // if show-draft in overridden, add that to the settings
            //partConfig.AddOverride(new LookUpInDictionary(DataSource.MyConfiguration, itemSettingsShowDrafts));

            #endregion

            // Check type because we renamed the DLL with the parts, and sometimes the old dll-name had been saved
            var dsType = dataQueryPart.DataSourceType;
            var dataSource = dataSourceFactory.Create(type: dsType, options: partConfig);
            try { dataSource.AddDebugInfo(dataQueryPart.Guid, dataQueryPart.Entity.GetBestTitle()); }
            catch { /* ignore */ }

            // new with errors
            try
            {
                var err = dataQueryPart.DataSourceInfo?.ErrorOrNull;
                if (dataQueryPart.DataSourceInfo?.ErrorOrNull != null && dataSource is Error errDs)
                {
                    errDs.Title = err.Title;
                    errDs.Message = err.Message;
                }
            }
            catch { }


            var partGuidStr = dataQueryPart.Guid.ToString();

            l.A($"add '{dsType.FullName}' as part#{dataQueryPart.Id}({partGuidStr.Substring(0, 6)}...)");
            dataSources.Add(partGuidStr, dataSource);
        }
        dataSources.Add("Out", outTarget);

        #endregion

        InitWirings(queryDef, dataSources);
        return l.Return(new(outTarget, dataSources), $"parts:{parts.Count}");
    }

    /// <summary>
    /// Init Stream Wirings between Query-Parts (Bottom-Up)
    /// </summary>
    private void InitWirings(QueryDefinition queryDef, IDictionary<string, IDataSource> dataSources) 
    {
        var l = Log.Fn($"count⋮{queryDef.Connections?.Count}");
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

        l.Done();
    }

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
                var errMsg = $"Trouble with connecting query from {wire.From}:{wire.Out} to {wire.To}:{wire.In}. ";
                if (!allDataSources.TryGetValue(wire.From, out var conSource))
                    throw new(errMsg + $"The source '{wire.From}' can't be found");
                if (!allDataSources.TryGetValue(wire.To, out var conTarget))
                    throw new(errMsg + $"The target '{wire.To}' can't be found");
                try
                {
                    // Temporary solution until immutable works perfectly
                    conTarget.DoWhileOverrideImmutable(() => conTarget.Attach(wire.In, conSource, wire.Out));
                    initializedWirings.Add(wire);
                    // In the end, inform caller that we did add some connections
                    connectionsWereAdded = true;
                }
                catch (Exception ex)
                {
                    throw new(errMsg, ex);
                }
            }
        }

        return connectionsWereAdded;
    }


    public QueryResult GetDataSourceForTesting(QueryDefinition queryDef, ILookUpEngine lookUps = null)
    {
        var l = Log.Fn<QueryResult>($"a#{queryDef.AppId}, pipe:{queryDef.Entity.EntityGuid} ({queryDef.Entity.EntityId})");
        var testValueProviders = queryDef.TestParameterLookUps;
        return l.ReturnAsOk(BuildQuery(queryDef, lookUps, testValueProviders));
    }


}
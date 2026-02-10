using ToSic.Eav.Apps;
using ToSic.Eav.Context.Sys.ZoneCulture;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp;
using ToSic.Eav.LookUp.Sources;
using ToSic.Eav.LookUp.Sources.Sys;
using ToSic.Eav.LookUp.Sys.Engines;
using ToSic.Eav.Services;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Eav.DataSource.Query.Sys;

/// <summary>
/// Factory to create a Data Query
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryFactory(
    LazySvc<IAppReaderFactory> appReaders,
    IDataSourcesService dataSourceFactory,
    IZoneCultureResolver cultureResolver,
    Generator<PassThrough> passThrough,
    IAppsCatalog appsCatalog,
    ICurrentContextUserPermissionsService userPermissions)
    : ServiceBase("DS.PipeFt",
        connect: [appReaders, cultureResolver, appsCatalog, dataSourceFactory, passThrough, userPermissions])
{
    private QueryWiringsHelper WiringsHelper => new(Log);

    //public QueryDefinition Create(IEntity entity, int appId)
    //    => queryDefinitionBuilder.Create(entity, appId);

    // 2026-02-10 2dm - moved to QueryManager / QueryService, as it's basically duplicate code
    ///// <summary>
    ///// Build a query-definition object based on the entity-ID defining the query
    ///// </summary>
    ///// <returns></returns>
    //public QueryDefinition GetQueryDefinition(int appId, int queryEntityId)
    //{
    //    var l = Log.Fn<QueryDefinition>($"def#{queryEntityId} for a#{appId}");
    //    try
    //    {
    //        //var app = appsCatalog.AppIdentity(appId);
    //        //var source = dataSourceFactory.CreateDefault(new DataSourceOptions { AppIdentityOrReader = app });
    //        //var appEntities = source.List;

    //        var appEntities = appReaders.Value.Get(appReaders.Value.AppIdentity(appId)).List;

    //        // use findRepo, as it uses the cache, which gives the list of all items
    //        var dataQuery = appEntities.FindRepoId(queryEntityId);
    //        if (dataQuery == null)
    //            throw new KeyNotFoundException($"QueryEntity with ID {queryEntityId} not found on AppId {appId}");
    //        var result = Create(dataQuery, appId);
    //        return l.Return(result);
    //    }
    //    catch (KeyNotFoundException)
    //    {
    //        throw l.Ex(new Exception("QueryEntity not found with ID " + queryEntityId + " on AppId " + appId));
    //    }
    //}


    public QueryResult BuildQuery(
        QueryDefinition queryDef,
        ILookUpEngine? lookUpEngineToClone,
        List<ILookUp> overrideLookUps) 
    {
        var l = Log.Fn<QueryResult>($"{queryDef.Title}({queryDef.Id}), hasLookUp:{lookUpEngineToClone != null}, overrides: {overrideLookUps?.Count}");
        #region prepare shared / global value providers
            
        var showDrafts = userPermissions.UserPermissions().ShowDraftData;
        if (queryDef.ParamsLookUp is LookUpInDictionary paramsLookup)
            paramsLookup.Properties[QueryConstants.ParamsShowDraftsKey] = showDrafts.ToString();

        // centralizing building of the primary configuration template for each part
        var baseLookUp = new LookUpEngine(lookUpEngineToClone, Log, sources: [queryDef.ParamsLookUp], overrides: overrideLookUps);

        #endregion

        #region Load Query Entity and Query Parts

        // tell the primary-out that it has this guid, for better debugging
        var passThroughLookUp = new LookUpEngine(baseLookUp, Log);
        IDataSource outTarget = passThrough.New().Init(passThroughLookUp);
        if (outTarget.Guid == Guid.Empty)
            outTarget.AddDebugInfo(queryDef.Guid, null);

        #endregion

        #region Load Parameters needed for all parts

        var appIdentity = appsCatalog.AppIdentity(queryDef.AppId);
        var dimensions = cultureResolver.SafeLanguagePriorityCodes();

        #endregion

        #region init all DataQueryParts

        l.A($"add parts to pipe#{queryDef.Id} ");
        var dataSources = new Dictionary<string, IDataSource>();
        var parts = queryDef.Parts;
        l.A($"parts:{parts.Count}");
			
        // More logging in unexpected case that we do not have parts.
        if (parts.Count == 0)
            l.A($"qd.Entity.Metadata:{(queryDef as ICanBeEntity).Entity.Metadata.Count()}");

        foreach (var dataQueryPart in parts)
        {
            #region Init Configuration Provider

            var querySpecsLookUp = new LookUpInQueryPartMetadata(
                DataSourceConstants.MyConfigurationSourceName,
                (dataQueryPart as ICanBeEntity).Entity,
                dimensions
            );
            var partEngine = new LookUpEngine(baseLookUp, Log, sources: [querySpecsLookUp]);
            // add / set item part configuration
            //partLookUp.Add(querySpecsLookUp);
            var partOptions = new DataSourceOptions
            {
                AppIdentityOrReader = appIdentity,
                LookUp = partEngine,
            };

            #endregion

            // Check type because we renamed the DLL with the parts, and sometimes the old dll-name had been saved
            var dsType = dataQueryPart.DataSourceType;
            var dataSource = dataSourceFactory.Create(type: dsType, options: partOptions);
            try { dataSource.AddDebugInfo(dataQueryPart.Guid, dataQueryPart.Title); }
            catch { /* ignore */ }

            // new with errors
            try
            {
                var err = dataQueryPart.DataSourceInfo?.ErrorOrNull;
                if (err != null && dataSource is Error errDs)
                {
                    errDs.Title = err.Title;
                    errDs.Message = err.Message;
                }
            }
            catch { /* ignore */ }


            var partGuidStr = dataQueryPart.Guid.ToString();

            l.A($"add '{dsType.FullName}' as part#{dataQueryPart.Id}({partGuidStr.Substring(0, 6)}...)");
            dataSources.Add(partGuidStr, dataSource);
        }
        dataSources.Add("Out", outTarget);

        #endregion

        WiringsHelper.InitWirings(queryDef, dataSources);
        return l.Return(new(outTarget, dataSources), $"parts:{parts.Count}");
    }






    public QueryResult GetDataSourceForTesting(QueryDefinition queryDef, ILookUpEngine? lookUps = null)
    {
        var l = Log.Fn<QueryResult>($"a#{queryDef.AppId}, pipe:{queryDef.Guid} ({queryDef.Id})");
        var testValueProviders = queryDef.TestParameterLookUps;
        return l.ReturnAsOk(BuildQuery(queryDef, lookUps, testValueProviders));
    }

}
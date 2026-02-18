using System.Configuration;
using ToSic.Eav.Apps;
using ToSic.Eav.Context.Sys.ZoneCulture;
using ToSic.Eav.DataSource.Sys.Streams;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp;
using ToSic.Eav.LookUp.Sources;
using ToSic.Eav.LookUp.Sources.Sys;
using ToSic.Eav.LookUp.Sys.Engines;
using ToSic.Eav.Services;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Eav.DataSource.Query.Sys;

/// <summary>
/// Factory to create a Data Query based on Query Definitions.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryFactory(
    LazySvc<IAppReaderFactory> appReaders,
    IDataSourcesService dataSourceFactory,
    IZoneCultureResolver cultureResolver,
    Generator<PassThrough, IDataSourceOptions> genPassThrough,
    IAppsCatalog appsCatalog,
    ICurrentContextUserPermissionsService userPermissions)
    : ServiceBase("DS.PipeFt",
        connect: [appReaders, cultureResolver, appsCatalog, dataSourceFactory, genPassThrough, userPermissions])
{
    private QueryWiringsHelper WiringsHelper => new(Log);

    /// <summary>
    /// Create a normal query, and attach/resolve any parameters from the query definition.
    /// </summary>
    /// <param name="queryDef"></param>
    /// <param name="lookUpEngineToClone"></param>
    /// <returns></returns>
    public QueryFactoryResult CreateWithParams(QueryDefinition queryDef, ILookUpEngine lookUpEngineToClone)
    {
        var l = Log.Fn<QueryFactoryResult>(message: $"Query: '{queryDef.Title}'", timer: true);
        // Step 1: Resolve the params from outside, where x=[Params:y] should come from the outer Params
        // and the current In
        var resolvedParams = lookUpEngineToClone.LookUp(queryDef.ParamsDic);

        // now provide an override source for this
        var paramsOverride = new LookUpInDictionary(DataSourceConstants.ParamsSourceName, resolvedParams);
        var queryInfos = Create(queryDef, lookUpEngineToClone, [paramsOverride]);
        return l.Return(queryInfos);
    }

    /// <summary>
    /// Generate a query but set test parameters. Used in Visual Query.
    /// </summary>
    /// <param name="queryDef"></param>
    /// <param name="lookUps"></param>
    /// <returns></returns>
    public QueryFactoryResult CreateWithTestParams(QueryDefinition queryDef, ILookUpEngine? lookUps = null)
    {
        var l = Log.Fn<QueryFactoryResult>($"a#{queryDef.AppId}, pipe:{queryDef.Guid} ({queryDef.Id})");
        var testValueProviders = queryDef.TestParameterLookUps;
        return l.ReturnAsOk(Create(queryDef, lookUps, testValueProviders));
    }

    private QueryFactoryResult Create(QueryDefinition queryDef, ILookUpEngine? lookUpEngineToClone, List<ILookUp> overrideLookUps) 
    {
        var l = Log.Fn<QueryFactoryResult>($"{queryDef.Title}({queryDef.Id}), hasLookUp:{lookUpEngineToClone != null}, overrides: {overrideLookUps?.Count}");
        #region prepare shared / global value providers
            
        var showDrafts = userPermissions.UserPermissions().ShowDraftData;
        if (queryDef.ParamsLookUp is LookUpInDictionary paramsLookup)
            paramsLookup.Properties[QueryConstants.ParamsShowDraftsKey] = showDrafts.ToString();

        // centralizing building of the primary configuration template for each part
        var baseLookUp = new LookUpEngine(lookUpEngineToClone, Log, sources: [queryDef.ParamsLookUp], overrides: overrideLookUps);

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
            var partOptions = new DataSourceOptions
            {
                AppIdentityOrReader = appIdentity,
                LookUp = partEngine,
            };

            #endregion

            // Check type because we renamed the DLL with the parts, and sometimes the old dll-name had been saved
            var dsType = dataQueryPart.DataSourceType;
            var dataSource = dataSourceFactory.Create(type: dsType, options: partOptions);
            try
            {
                dataSource.AddDebugInfo(dataQueryPart.Guid, dataQueryPart.Title);
            }
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

            l.A($"add '{dsType.FullName}' as part#{dataQueryPart.Id} ({partGuidStr})");
            dataSources.Add(partGuidStr, dataSource);
        }

        #region Load Query Entity and Query Parts

        // tell the primary-out that it has this guid, for better debugging
        var passThroughLookUp = new LookUpEngine(baseLookUp, Log);
        var outTarget = genPassThrough.New(new DataSourceOptions
        {
            AppIdentityOrReader = appIdentity,
            LookUp = passThroughLookUp
        });
        if (outTarget.Guid == Guid.Empty)
            outTarget.AddDebugInfo(queryDef.Guid, null);

        #endregion

        dataSources.Add("Out", outTarget);

        #endregion

        WiringsHelper.InitWirings(queryDef, dataSources);
        return l.Return(new(outTarget, dataSources), $"parts:{parts.Count}");
    }


}
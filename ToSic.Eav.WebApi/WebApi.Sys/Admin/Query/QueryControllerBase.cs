using System.Diagnostics;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Query.Sys;
using ToSic.Eav.DataSource.Query.Sys.Inspect;
using ToSic.Eav.DataSource.Sys.Catalog;
using ToSic.Eav.DataSources;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.LookUp.Sys.Engines;
using ToSic.Eav.Metadata;
using ToSic.Eav.Serialization.Sys;
using ToSic.Eav.Serialization.Sys.Json;
using ToSic.Eav.WebApi.Sys.Dto;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;

namespace ToSic.Eav.WebApi.Sys.Admin.Query;

/// <inheritdoc />
/// <summary>
/// Web API Controller for the Pipeline Designer UI.
///
/// It's just a base controller, because some methods need to be added at the SXC level which don't exist in the EAV.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class QueryControllerBase<TImplementation>(
    QueryControllerBase<TImplementation>.Dependencies services,
    string logName,
    object[]? connect = default)
    : ServiceBase<QueryControllerBase<TImplementation>.Dependencies>(services, logName, connect: connect)
    where TImplementation : QueryControllerBase<TImplementation>
{

    #region Constructor / DI / Services

    public record Dependencies(
        QueryFactory QueryFactory,
        LazySvc<ConvertToEavLight> EntToDicLazy,
        LazySvc<QueryInspectionService> QueryInfoLazy,
        LazySvc<DataSourceCatalog> DataSourceCatalogLazy,
        Generator<JsonSerializer> JsonSerializer,
        Generator<PassThrough> PassThrough,
        LazySvc<QueryManager> QueryManager,
        Generator<IAppReaderFactory> AppStates,
        GenWorkBasic<WorkQueryMod> WorkUnitQueryMod,
        GenWorkBasic<WorkQueryCopy> WorkUnitQueryCopy)
        : DependenciesRecord(connect:
        [
            QueryFactory, EntToDicLazy, QueryInfoLazy, DataSourceCatalogLazy, JsonSerializer, PassThrough, QueryManager,
            AppStates, WorkUnitQueryMod, WorkUnitQueryCopy
        ]);

    #endregion

    /// <summary>
    /// Get a Pipeline with DataSources
    /// </summary>
    public QueryDefinitionDto Get(int appId, int? id = null)
    {
        var l = Log.Fn<QueryDefinitionDto>($"a#{appId}, id:{id}");

        if (!id.HasValue)
            return l.Return(new(), "no id, empty");

        var appState = Services.AppStates.New().Get(appId);
        var qDef = Services.QueryManager.Value.GetDefinition(appState, id.Value);

        #region Deserialize some Entity-Values

        var pipeline = (qDef as ICanBeEntity).Entity.AsDictionary();
        pipeline[nameof(QueryDefinition.StreamWiring)] = qDef.Connections;

        var converter = Services.EntToDicLazy.Value;
        converter.Type.Serialize = true;
        converter.Type.WithDescription = true;
        converter.WithGuid = true;

        //foreach (var part in qDef.Parts)
        //{
        //    var partDto = part.AsDictionary();
        //    var metadata = appState.Metadata.GetMetadata(TargetTypes.Entity, part.Guid);
        //    partDto.Add("Metadata", converter.Convert(metadata));
        //    queryDto.DataSources.Add(partDto);
        //}

        var dataSources = qDef.Parts
            .Select(part =>
            {
                var partDto = part.AsDictionary();
                var metadata = appState.Metadata.GetMetadata(TargetTypes.Entity, part.Guid);
                partDto.Add("Metadata", converter.Convert(metadata));
                return partDto;
            })
            .ToListOpt();

        var queryDto = new QueryDefinitionDto
        {
            Pipeline = pipeline!,
            DataSources = dataSources!,
        };


        #endregion

        return l.ReturnAsOk(queryDto);
    }

    /// <summary>
    /// Get installed DataSources from .NET Runtime but only those with [PipelineDesigner Attribute]
    /// </summary>
    /// <param name="appIdentity"></param>
    public IEnumerable<DataSourceDto> DataSources(AppIdentity appIdentity)
    {
        var l = Log.Fn<IEnumerable<DataSourceDto>>($"a#{appIdentity.AppId}");
        var dsCat = Services.DataSourceCatalogLazy.Value;
        var installedDataSources = Services.DataSourceCatalogLazy.Value.GetAll(true, appIdentity.AppId);

        var result = installedDataSources
            .Select(ds =>
            {
                var outNames = ds.VisualQuery?.DynamicOut == true
                    ? null
                    : dsCat.GetOutStreamNames(ds);
                return new DataSourceDto(ds, outNames);
            })
            .OrderBy(ds => ds.TypeNameForUi) // sort for better debugging in F12
            .ToListOpt();

        return l.Return(result, result.Count.ToString());
    }

    /// <summary>
    /// Save Pipeline
    /// </summary>
    /// <param name="data">JSON object { pipeline: pipeline, dataSources: dataSources }</param>
    /// <param name="appId">AppId this Pipeline belongs to</param>
    /// <param name="id">PipelineEntityId</param>
    public QueryDefinitionDto Save(QueryDefinitionDto data, int appId, int id)
    {
        var l = Log.Fn<QueryDefinitionDto>($"save pipe: a#{appId}, id#{id}");
        // assemble list of all new data-source guids, for later re-mapping when saving
        var newDsGuids = data.DataSources
            .Where(d => d.ContainsKey("EntityGuid"))
            .Select(d => d["EntityGuid"].ToString()!)
            .Where(g => g != "Out" && !g.StartsWith("unsaved"))
            .Select(Guid.Parse)
            .ToListOpt();

        // Update Pipeline Entity with new Wirings etc.
        var wiringString = data.Pipeline[nameof(QueryDefinition.StreamWiring)]?.ToString() ?? "";
        var wirings =
            SystemJsonSerializer.Deserialize<List<QueryWire>>(wiringString, JsonOptions.UnsafeJsonWithoutEncodingHtml)
            ?? [];

        Services.WorkUnitQueryMod.New(appId: appId).Update(id, data.DataSources, newDsGuids, data.Pipeline, wirings);

        return l.ReturnAsOk(Get(appId, id));
    }

    /// <summary>
    /// Query the Result of a Pipeline using Test-Parameters
    /// </summary>
    protected QueryRunDto DebugStream(int appId, int id, int top, ILookUpEngine lookUps, string from, string streamName)
    {
        return RunDevInternal(appId, id, lookUps, top, GetSubStream);

        IDataSource GetSubStream(QueryFactoryResult builtQuery)
        {
            // Find the DataSource
            if (!builtQuery.DataSources.TryGetValue(from, out var source))
                throw new($"Can't find source with name '{from}'");

            if (!source.Out.TryGetValue(streamName, out var resultStream))
                throw new($"Can't find stream '{streamName}' on source '{from}'");

            // Repackage as DataSource since that's expected / needed
            var passThroughDs = Services.PassThrough.New();
            passThroughDs.Attach(streamName, resultStream);

            return passThroughDs;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="id"></param>
    /// <param name="lookUps"></param>
    /// <param name="top"></param>
    /// <param name="partLookup">This retrieves which part of the query should be provided - for scenarios where only a single stream in the query is analyzed</param>
    /// <returns></returns>
    protected QueryRunDto RunDevInternal(int appId, int id, ILookUpEngine lookUps, int top,
        Func<QueryFactoryResult, IDataSource> partLookup) 
    {
        var l = Log.Fn<QueryRunDto>($"a#{appId}, {nameof(id)}:{id}, top: {top}");

        // Get the query, run it and track how much time this took
        // var qDef = Services.QueryFactory.GetQueryDefinition(appId, id);
        var qDef = services.QueryManager.Value.GetDefinition(appId, id);
        var builtQuery = Services.QueryFactory.BuildWithTestParams(qDef, lookUps: lookUps);
        var outSource = builtQuery.Main;

        // New v17 experimental with special fields
        var systemQueryOptions = new QueryODataParams(outSource.Configuration).SystemQueryOptions;

        var timer = new Stopwatch();
        timer.Start();
        var results = Log.Quick(message: "Serialize", timer: true, func: () =>
        {
            var converter = Services.EntToDicLazy.Value;
            //converter.WithGuid = true;
            converter.MaxItems = top;
            converter.AddSelectFields(systemQueryOptions.Select.ToListOpt());

            // Use passed in function to select the part to serialize
            var part = partLookup(builtQuery);
            var converted = converter.Convert(part);
            return converted;
        });
        timer.Stop();

        // Now get some more debug info
        var debugInfo = Services.QueryInfoLazy.Value.BuildQueryInfo(qDef, outSource);

        // ...and return the results
        var result = new QueryRunDto
        {
            Query = results,
            Streams = debugInfo.Streams
                .Select(si =>
                {
                    if (si.ErrorData is IEntity errorEntity)
                        si.ErrorData = Services.EntToDicLazy.Value.Convert(errorEntity);
                    return si;
                })
                .ToListOpt(),
            Sources = debugInfo.Sources,
            QueryTimer = new()
            {
                Milliseconds = timer.ElapsedMilliseconds,
                Ticks = timer.ElapsedTicks
            }
        };
        return l.ReturnAsOk(result);
    }


    /// <summary>
    /// Clone a Pipeline with all DataSources and their configurations
    /// </summary>
    public void Clone(int appId, int id) => Services.WorkUnitQueryCopy.New(appId: appId).SaveCopy(id);
        

    public bool Import(EntityImportDto args)
    {
        var l = Log.Fn<bool>(args.DebugInfo);
        try
        {
            var workUnit = Services.WorkUnitQueryCopy.New(appId: args.AppId);
            var deser = Services.JsonSerializer.New().SetApp(workUnit.AppWorkCtx.AppReader);
            var ents = deser.Deserialize(args.GetContentString());
            var qdef = Services.QueryManager.Value.GetDefinition(args.AppId, ents);
            workUnit.SaveCopy(qdef);

            return l.ReturnTrue();
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            throw new($"Couldn't import - {ex?.Message ?? "probably bad file format"}.", ex);
        }
    }

}
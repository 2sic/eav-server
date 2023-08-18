﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Catalog;
using ToSic.Eav.DataSource.Debug;
using ToSic.Eav.DataSource.Query;
using ToSic.Eav.DataSources;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Lib.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.Metadata;
using ToSic.Eav.Serialization;
using ToSic.Eav.WebApi.Dto;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using Connection = ToSic.Eav.DataSource.Query.Connection;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;

namespace ToSic.Eav.WebApi.Admin.Query
{
    /// <inheritdoc />
    /// <summary>
    /// Web API Controller for the Pipeline Designer UI
    /// </summary>
    public abstract class QueryControllerBase<TImplementation> : ServiceBase<QueryControllerBase<TImplementation>.MyServices> where TImplementation : QueryControllerBase<TImplementation>
    {

        #region Constructor / DI / Services

        public class MyServices : MyServicesBase
        {
            public LazySvc<AppManager> AppManagerLazy { get; }
            /// <summary>
            /// The lazy reader should only be used in the Definition - it's important that it's a new object
            /// when used, to ensure it has the changes previously saved
            /// </summary>
            public LazySvc<AppRuntime> AppReaderLazy { get; }
            public QueryBuilder QueryBuilder { get; }
            public LazySvc<ConvertToEavLight> EntToDicLazy { get; }
            public LazySvc<QueryInfo> QueryInfoLazy { get; }
            public LazySvc<DataSourceCatalog> DataSourceCatalogLazy { get; }
            public Generator<ToSic.Eav.ImportExport.Json.JsonSerializer> JsonSerializer { get; }
            public Generator<PassThrough> PassThrough { get; }

            public MyServices(LazySvc<AppManager> appManagerLazy,
                LazySvc<AppRuntime> appReaderLazy,
                QueryBuilder queryBuilder,
                LazySvc<ConvertToEavLight> entToDicLazy,
                LazySvc<QueryInfo> queryInfoLazy,
                LazySvc<DataSourceCatalog> dataSourceCatalogLazy,
                Generator<ToSic.Eav.ImportExport.Json.JsonSerializer> jsonSerializer,
                Generator<PassThrough> passThrough)
            {
                ConnectServices(
                    AppManagerLazy = appManagerLazy,
                    AppReaderLazy = appReaderLazy,
                    QueryBuilder = queryBuilder,
                    EntToDicLazy = entToDicLazy,
                    QueryInfoLazy = queryInfoLazy,
                    DataSourceCatalogLazy = dataSourceCatalogLazy,
                    JsonSerializer = jsonSerializer,
                    PassThrough = passThrough
                );
            }
        }



        protected QueryControllerBase(MyServices services, string logName) : base(services, logName)
        {
            QueryBuilder = services.QueryBuilder;
        }
        private AppManager _appManager;
        private QueryBuilder QueryBuilder { get; }

        #endregion

        public TImplementation Init(int appId)
        {
            _appId = appId;
            if (appId != 0) // if 0, then no context is available or used
                _appManager = Services.AppManagerLazy.Value.Init(appId);
            return this as TImplementation;
        }

        private int _appId;



        /// <summary>
        /// Get a Pipeline with DataSources
        /// </summary>
        public QueryDefinitionDto Get(int appId, int? id = null)
        {
            var l = Log.Fn<QueryDefinitionDto>($"a#{appId}, id:{id}");
            var query = new QueryDefinitionDto();

            if (!id.HasValue) return l.Return(query, "no id, empty");

            var reader = Services.AppReaderLazy.Value.Init(appId);
            var qDef = reader.Queries.Get(id.Value);

            #region Deserialize some Entity-Values

            query.Pipeline = qDef.Entity.AsDictionary();
            query.Pipeline[QueryConstants.QueryStreamWiringAttributeName] = qDef.Connections;

            var converter = Services.EntToDicLazy.Value;
            converter.Type.Serialize = true;
            converter.Type.WithDescription = true;
            converter.WithGuid = true;

            foreach (var part in qDef.Parts)
            {
                var partDto = part.AsDictionary();
                var metadata = reader.AppState.GetMetadata(TargetTypes.Entity, part.Guid);
                partDto.Add("Metadata", converter.Convert(metadata));
                query.DataSources.Add(partDto);
            }

            #endregion

            return l.ReturnAsOk(query);
        }

        /// <summary>
        /// Get installed DataSources from .NET Runtime but only those with [PipelineDesigner Attribute]
        /// </summary>
        public IEnumerable<DataSourceDto> DataSources()
        {
            var l = Log.Fn<IEnumerable<DataSourceDto>>($"a#{_appId}");
            var dsCat = Services.DataSourceCatalogLazy.Value;
            var installedDataSources = Services.DataSourceCatalogLazy.Value.GetAll(true, _appId);

            var result = installedDataSources
                .Select(ds => new DataSourceDto(ds, ds.VisualQuery?.DynamicOut == true ? null : dsCat.GetOutStreamNames(ds)))
                .ToList();

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
            var newDsGuids = data.DataSources.Where(d => d.ContainsKey("EntityGuid"))
                .Select(d => d["EntityGuid"].ToString())
                .Where(g => g != "Out" && !g.StartsWith("unsaved"))
                .Select(Guid.Parse)
                .ToList();

            // Update Pipeline Entity with new Wirings etc.
            var wiringString = data.Pipeline[QueryConstants.QueryStreamWiringAttributeName]?.ToString() ?? "";
            var wirings =
                SystemJsonSerializer.Deserialize<List<Connection>>(wiringString, JsonOptions.UnsafeJsonWithoutEncodingHtml)
                ?? new List<Connection>();

            _appManager.Queries.Update(id, data.DataSources, newDsGuids, data.Pipeline, wirings);

            return l.ReturnAsOk(Get(appId, id));
        }

        /// <summary>
        /// Query the Result of a Pipeline using Test-Parameters
        /// </summary>
        protected QueryRunDto DebugStream(int appId, int id, int top, LookUpEngine lookUps, string from, string streamName)
        {
            IDataSource GetSubStream((IDataSource, Dictionary<string, IDataSource>) builtQuery)
            {
                // Find the DataSource
                if (!builtQuery.Item2.ContainsKey(from))
                    throw new Exception($"Can't find source with name '{from}'");

                var source = builtQuery.Item2[from];
                if (!source.Out.ContainsKey(streamName))
                    throw new Exception($"Can't find stream '{streamName}' on source '{from}'");

                var resultStream = source.Out[streamName];
                
                // Repackage as DataSource since that's expected / needed
                var passThroughDs = Services.PassThrough.New();
                passThroughDs.Attach(streamName, resultStream);

                return passThroughDs;
            }

            return RunDevInternal(appId, id, lookUps, top, GetSubStream);
        }

        protected QueryRunDto RunDevInternal(int appId, int id, LookUpEngine lookUps, int top,
            Func<(IDataSource Main, Dictionary<string, IDataSource> DataSources), IDataSource> partLookup) 
        {
            var l = Log.Fn<QueryRunDto>($"a#{appId}, {nameof(id)}:{id}, top: {top}");
            // Get the query, run it and track how much time this took
            var qDef = QueryBuilder.GetQueryDefinition(appId, id);
            var builtQuery = QueryBuilder.GetDataSourceForTesting(qDef, lookUps: lookUps);
            var outSource = builtQuery.Item1;


            var timer = new Stopwatch();
            timer.Start();
            var results = Log.Func(message: "Serialize", timer: true, func: () =>
            {
                var converter = Services.EntToDicLazy.Value;
                converter.WithGuid = true;
                converter.MaxItems = top;
                var converted = converter.Convert(partLookup(builtQuery));
                return (converted, "ok");
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
                    .ToList(),
                Sources = debugInfo.Sources,
                QueryTimer = new QueryTimerDto
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
        public void Clone(int appId, int id) => _appManager.Queries.SaveCopy(id);
		

		/// <summary>
		/// Delete a Pipeline with the Pipeline Entity, Pipeline Parts and their Configurations. Stops if the if the Pipeline Entity has relationships to other Entities.
		/// </summary>
		public bool Delete(int id) => _appManager.Queries.Delete(id);


        public bool Import(EntityImportDto args)
        {
            var l = Log.Fn<bool>(args.DebugInfo);
            try
            {
                var deser = Services.JsonSerializer.New().SetApp(_appManager.AppState);
                var ents = deser.Deserialize(args.GetContentString());
                var qdef = QueryBuilder.Create(ents, args.AppId);
                _appManager.Queries.SaveCopy(qdef);

                return l.ReturnTrue();
            }
            catch (Exception ex)
            {
                l.Ex(ex);
                throw new Exception($"Couldn't import - {ex?.Message ?? "probably bad file format"}.", ex);
            }
        }

    }
}
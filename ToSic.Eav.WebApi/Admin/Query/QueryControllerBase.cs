using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.Metadata;
using ToSic.Eav.Serialization;
using ToSic.Eav.WebApi.Dto;
using Connection = ToSic.Eav.DataSources.Queries.Connection;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ToSic.Eav.WebApi.Admin.Query
{
    /// <inheritdoc />
    /// <summary>
    /// Web API Controller for the Pipeline Designer UI
    /// </summary>
    public abstract class QueryControllerBase<TImplementation> : HasLog where TImplementation : QueryControllerBase<TImplementation>
    {
        protected QueryControllerBase(QueryControllerDependencies dependencies, string logName) : base(logName)
        {
            _dependencies = dependencies;
            QueryBuilder = dependencies.QueryBuilder;
            QueryBuilder.Init(Log);
        }
        private readonly QueryControllerDependencies _dependencies;
        private AppManager _appManager;
        private QueryBuilder QueryBuilder { get; }


        public TImplementation Init(int appId)
        {
            if (appId != 0) // if 0, then no context is available or used
                _appManager = _dependencies.AppManagerLazy.Value.Init(Log).Init(appId);
            return this as TImplementation;
        }



        /// <summary>
        /// Get a Pipeline with DataSources
        /// </summary>
		public QueryDefinitionDto Get(int appId, int? id = null)
        {
            Log.A($"get pipe a#{appId}, id:{id}");
            var query = new QueryDefinitionDto();

            if (!id.HasValue) return query;

            var reader = _dependencies.AppReaderLazy.Value.Init(Log).Init(appId, false);
            var qDef = reader.Queries.Get(id.Value);

            #region Deserialize some Entity-Values

            query.Pipeline = qDef.Entity.AsDictionary();
            query.Pipeline[Constants.QueryStreamWiringAttributeName] = qDef.Connections;

            var converter = _dependencies.EntToDicLazy.Value;
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

            return query;
        }

        /// <summary>
        /// Get installed DataSources from .NET Runtime but only those with [PipelineDesigner Attribute]
        /// </summary>
        public IEnumerable<DataSourceDto> DataSources()
        {
            var dsCatalog = _dependencies.DataSourceCatalogLazy.Value.Init(Log);

            var callLog = Log.Fn<IEnumerable<DataSourceDto>>();
            var installedDataSources = DataSourceCatalog.GetAll(true);

            var result = installedDataSources
                .Select(dsInfo => new DataSourceDto(dsInfo.Type.Name, dsInfo.VisualQuery)
                {
                    TypeNameForUi = dsInfo.Type.FullName,
                    PartAssemblyAndType = dsInfo.Name,
                    Identifier = dsInfo.Name,
                    Out = dsInfo.VisualQuery?.DynamicOut == true ? null : dsCatalog.GetOutStreamNames(dsInfo)
                })
                .ToList();

            return callLog.Return(result, result.Count.ToString());

        }

        /// <summary>
        /// Save Pipeline
        /// </summary>
        /// <param name="data">JSON object { pipeline: pipeline, dataSources: dataSources }</param>
        /// <param name="appId">AppId this Pipeline belongs to</param>
        /// <param name="id">PipelineEntityId</param>
        public QueryDefinitionDto Save(QueryDefinitionDto data, int appId, int id)
		{
		    Log.A($"save pipe: a#{appId}, id#{id}");

            // assemble list of all new data-source guids, for later re-mapping when saving
            var newDsGuids = data.DataSources.Where(d => d.ContainsKey("EntityGuid"))
                .Select(d => d["EntityGuid"].ToString())
                .Where(g => g != "Out" && !g.StartsWith("unsaved"))
                .Select(Guid.Parse)
                .ToList();

            // Update Pipeline Entity with new Wirings etc.
            var wiringString = data.Pipeline[Constants.QueryStreamWiringAttributeName]?.ToString() ?? "";
            var wirings = JsonSerializer.Deserialize<List<Connection>>(wiringString, JsonOptions.UnsafeJsonWithoutEncodingHtml)
                ?? new List<Connection>();

            _appManager.Queries.Update(id, data.DataSources, newDsGuids, data.Pipeline, wirings);

		    return Get(appId, id);
		}

        /// <summary>
        /// Query the Result of a Pipeline using Test-Parameters
        /// </summary>
        protected QueryRunDto DebugStream(int appId, int id, int top, LookUpEngine lookUps, string from, string streamName)
        {
            IDataSource GetSubStream(Tuple<IDataSource, Dictionary<string, IDataSource>> builtQuery)
            {
                // Find the DataSource
                if (!builtQuery.Item2.ContainsKey(from))
                    throw new Exception($"Can't find source with name '{from}'");

                var source = builtQuery.Item2[from];
                if (!source.Out.ContainsKey(streamName))
                    throw new Exception($"Can't find stream '{streamName}' on source '{from}'");

                var resultStream = source.Out[streamName];
                
                // Repackage as DataSource since that's expected / needed
                var passThroughDs = new PassThrough();
                passThroughDs.Attach(streamName, resultStream);

                return passThroughDs;
            }

            return RunDevInternal(appId, id, lookUps, top, GetSubStream);
        }

        protected QueryRunDto RunDevInternal(int appId, int id, LookUpEngine lookUps, int top, Func<Tuple<IDataSource, Dictionary<string, IDataSource>>, IDataSource> partLookup)
		{
            var wrapLog = Log.Fn<QueryRunDto>($"a#{appId}, {nameof(id)}:{id}, top: {top}");

            // Get the query, run it and track how much time this took
		    var qDef = QueryBuilder.GetQueryDefinition(appId, id);
			var builtQuery = QueryBuilder.GetDataSourceForTesting(qDef, true, lookUps);
            var outSource = builtQuery.Item1;
            
            
            var serializeWrap = Log.Fn("Serialize", startTimer: true);
            var timer = new Stopwatch();
            timer.Start();
            var converter = _dependencies.EntToDicLazy.Value;
            converter.WithGuid = true;//.EnableGuids();
            converter.MaxItems = top;
		    var results = converter.Convert(partLookup(builtQuery));
            timer.Stop();
            serializeWrap.Done("ok");

            // Now get some more debug info
            var debugInfo = _dependencies.QueryInfoLazy.Value.BuildQueryInfo(qDef, outSource, Log);

            // ...and return the results
			return wrapLog.Return(new QueryRunDto
			{
				Query = results, 
				Streams = debugInfo.Streams.Select(si =>
                {
                    if(si.ErrorData != null && si.ErrorData is IEntity errorEntity)
                        si.ErrorData = _dependencies.EntToDicLazy.Value.Convert(errorEntity);
                    return si;
                }).ToList(),
				Sources = debugInfo.Sources,
                QueryTimer = new QueryTimerDto { 
                    Milliseconds = timer.ElapsedMilliseconds,
                    Ticks = timer.ElapsedTicks
                }
			});
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
            try
            {
                Log.A("import content" + args.DebugInfo);

                var deser = _dependencies.JsonSerializer.New().Init(_appManager.AppState, Log);
                var ents = deser.Deserialize(args.GetContentString());
                var qdef = new QueryDefinition(ents, args.AppId, Log);
                _appManager.Queries.SaveCopy(qdef);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't import - {ex?.Message ?? "probably bad file format"}.", ex);
            }
        }

    }
}
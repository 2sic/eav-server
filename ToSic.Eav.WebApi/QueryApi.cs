using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Eav.DataSources.Debug;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Eav.WebApi.Dto;
using Connection = ToSic.Eav.DataSources.Queries.Connection;

namespace ToSic.Eav.WebApi
{
	/// <inheritdoc />
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public abstract class QueryApi : HasLog
    {
        public class Dependencies
        {
            public Lazy<AppManager> AppManagerLazy { get; }
            /// <summary>
            /// The lazy reader should only be used in the Definition - it's important that it's a new object
            /// when used, to ensure it has the changes previously saved
            /// </summary>
            public Lazy<AppRuntime> AppReaderLazy { get; }
            public QueryBuilder QueryBuilder { get; }
            public Lazy<ConvertToEavLight> EntToDicLazy { get; }
            public Lazy<QueryInfo> QueryInfoLazy { get; }
            public Lazy<DataSourceCatalog> DataSourceCatalogLazy { get; }
            public Lazy<MetadataBackend> MetadataBackendLazy { get; }

            public Dependencies(Lazy<AppManager> appManagerLazy,
                Lazy<AppRuntime> appReaderLazy,
                QueryBuilder queryBuilder,
                Lazy<ConvertToEavLight> entToDicLazy,
                Lazy<QueryInfo> queryInfoLazy,
                Lazy<DataSourceCatalog> dataSourceCatalogLazy,
                Lazy<MetadataBackend> metadataBackendLazy)
            {
                AppManagerLazy = appManagerLazy;
                AppReaderLazy = appReaderLazy;
                QueryBuilder = queryBuilder;
                EntToDicLazy = entToDicLazy;
                QueryInfoLazy = queryInfoLazy;
                DataSourceCatalogLazy = dataSourceCatalogLazy;
                MetadataBackendLazy = metadataBackendLazy;
            }
        }

        public QueryBuilder QueryBuilder { get; }
        private readonly Dependencies _dependencies;

        private AppManager _appManager;

        protected QueryApi(
            Dependencies dependencies) : base("Api.EavQry")
        {
            _dependencies = dependencies;
            QueryBuilder = dependencies.QueryBuilder;
            QueryBuilder.Init(Log);
        }

        public QueryApi Init(int appId, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            if (appId != 0) // if 0, then no context is available or used
                _appManager = _dependencies.AppManagerLazy.Value.Init(appId, Log);
            return this;
        }



        /// <summary>
        /// Get a Pipeline with DataSources
        /// </summary>
		public QueryDefinitionDto Definition(int appId, int? id = null)
        {
            Log.Add($"get pipe a#{appId}, id:{id}");
            var query = new QueryDefinitionDto();

            if (!id.HasValue) return query;

            var reader = _dependencies.AppReaderLazy.Value.Init(appId, false, Log);
            var qDef = reader.Queries.Get(id.Value);

            var metadataBackend = _dependencies.MetadataBackendLazy.Value;

            #region Deserialize some Entity-Values

            query.Pipeline = qDef.Entity.AsDictionary();
            query.Pipeline[Constants.QueryStreamWiringAttributeName] = qDef.Connections;

            foreach (var part in qDef.Parts)
            {
                var partDto = part.AsDictionary();

                var metadata = reader.AppState.GetMetadata((int) TargetTypes.Entity, part.Guid)?.Select(m => m.AsDictionary());
                partDto.Add("Metadata", metadata);
                query.DataSources.Add(partDto);
            }

            #endregion

            return query;
        }

        public IEnumerable<DataSourceDto> DataSources()
        {
            var dsCatalog = _dependencies.DataSourceCatalogLazy.Value.Init(Log);

            var callLog = Log.Call<IEnumerable<DataSourceDto>>();
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

            return callLog(result.Count.ToString(), result);

        }

        /// <summary>
		/// Save Pipeline
		/// </summary>
		/// <param name="data">JSON object { pipeline: pipeline, dataSources: dataSources }</param>
		/// <param name="appId">AppId this Pipeline belongs to</param>
		/// <param name="id">PipelineEntityId</param>
		public QueryDefinitionDto Save(QueryDefinitionDto data, int appId, int id)
		{
		    Log.Add($"save pipe: a#{appId}, id#{id}");

            // assemble list of all new data-source guids, for later re-mapping when saving
            var newDsGuids = data.DataSources.Select(d => (string)d["EntityGuid"])
                .Where(g => g != "Out" && !g.StartsWith("unsaved"))
                .Select(Guid.Parse)
                .ToList();

            // Update Pipeline Entity with new Wirings etc.
            var wiringString = data.Pipeline[Constants.QueryStreamWiringAttributeName]?.ToString() ?? "";
            var wirings = JsonConvert.DeserializeObject<List<Connection>>(wiringString)
                ?? new List<Connection>();

            _appManager.Queries.Update(id, data.DataSources, newDsGuids, data.Pipeline, wirings);

		    return Definition(appId, id);
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
            var wrapLog = Log.Call($"a#{appId}, {nameof(id)}:{id}, top: {top}");

            // Get the query, run it and track how much time this took
		    var qDef = QueryBuilder.GetQueryDefinition(appId, id);
			var builtQuery = QueryBuilder.GetDataSourceForTesting(qDef, true, lookUps);
            var outSource = builtQuery.Item1;
            
            
            var serializeWrap = Log.Call("Serialize", useTimer: true);
            var timer = new Stopwatch();
            timer.Start();
            var converter = _dependencies.EntToDicLazy.Value;
            converter.WithGuid = true;//.EnableGuids();
            converter.MaxItems = top;
		    var results = converter.Convert(partLookup(builtQuery));
            timer.Stop();
            serializeWrap("ok");

            // Now get some more debug info
            var debugInfo = _dependencies.QueryInfoLazy.Value.BuildQueryInfo(qDef, outSource, Log);

            wrapLog(null);
            // ...and return the results
			return new QueryRunDto
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
			};
		}


        /// <summary>
        /// Clone a Pipeline with all DataSources and their configurations
        /// </summary>
        public void Clone(int appId, int id) => _appManager.Queries.SaveCopy(id);
		

		/// <summary>
		/// Delete a Pipeline with the Pipeline Entity, Pipeline Parts and their Configurations. Stops if the if the Pipeline Entity has relationships to other Entities.
		/// </summary>
		public bool Delete(/*int appId,*/ int id) => _appManager.Queries.Delete(id);


        public bool Import(EntityImportDto args)
        {
            try
            {
                Log.Add("import content" + args.DebugInfo);

                var deser = _appManager.ServiceProvider.Build<ImportExport.Json.JsonSerializer>().Init(_appManager.AppState, Log);
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
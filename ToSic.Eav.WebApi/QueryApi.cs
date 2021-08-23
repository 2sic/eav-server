using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Conversion;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Debug;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.Helpers;
using Connection = ToSic.Eav.DataSources.Queries.Connection;

namespace ToSic.Eav.WebApi
{
	/// <inheritdoc />
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public class QueryApi : HasLog
    {
        public QueryBuilder QueryBuilder { get; }
        private readonly Lazy<AppManager> _appManagerLazy;
        
        /// <summary>
        /// The lazy reader should only be used in the Definition - it's important that it's a new object
        /// when used, to ensure it has the changes previously saved
        /// </summary>
        private readonly Lazy<AppRuntime> _appReaderLazy;
        private readonly Lazy<EntitiesToDictionary> _entToDicLazy;
        private readonly Lazy<QueryInfo> _queryInfoLazy;
        private AppManager _appManager;

        public QueryApi(
            Lazy<AppManager> appManagerLazy, 
            Lazy<AppRuntime> appReaderLazy, 
            QueryBuilder queryBuilder, 
            Lazy<EntitiesToDictionary> entToDicLazy,
            Lazy<QueryInfo> queryInfoLazy
            ) : base("Api.EavQry")
        {
            QueryBuilder = queryBuilder;
            QueryBuilder.Init(Log);
            _appManagerLazy = appManagerLazy;
            _appReaderLazy = appReaderLazy;
            _entToDicLazy = entToDicLazy;
            _queryInfoLazy = queryInfoLazy;
        }

        public QueryApi Init(int appId, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            _appManager = _appManagerLazy.Value.Init(appId, Log);
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

            var reader = _appReaderLazy.Value.Init(State.Identity(null, appId), false, Log);
            var qDef = reader.Queries.Get(id.Value);

            #region Deserialize some Entity-Values

            query.Pipeline = qDef.Entity.AsDictionary();
            query.Pipeline[Constants.QueryStreamWiringAttributeName] = qDef.Connections;

            foreach (var part in qDef.Parts) 
                query.DataSources.Add(part.AsDictionary());

            #endregion

            return query;
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
		public QueryRunDto Run(int appId, int id, int top, LookUpEngine lookUps) 
            => DevRun(appId, id, lookUps, top, builtQuery => builtQuery.Item1);

        /// <summary>
        /// Query the Result of a Pipeline using Test-Parameters
        /// </summary>
        public QueryRunDto DebugStream(int appId, int id, int top, LookUpEngine lookUps, string from, string streamName)
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

            return DevRun(appId, id, lookUps, top, GetSubStream);
        }

        public QueryRunDto DevRun(int appId, int id, LookUpEngine lookUps, int top, Func<Tuple<IDataSource, Dictionary<string, IDataSource>>, IDataSource> partLookup)
		{
            var wrapLog = Log.Call($"a#{appId}, {nameof(id)}:{id}, top: {top}");

            // Get the query, run it and track how much time this took
		    var qDef = QueryBuilder.GetQueryDefinition(appId, id);
			var builtQuery = QueryBuilder.GetDataSourceForTesting(qDef, true, lookUps);
            var outSource = builtQuery.Item1;
            
            
            var serializeWrap = Log.Call("Serialize", useTimer: true);
            var timer = new Stopwatch();
            timer.Start();
            var converter = _entToDicLazy.Value.EnableGuids();
            converter.MaxItems = top;
		    var results = converter.Convert(partLookup(builtQuery));
            timer.Stop();
            serializeWrap("ok");

            // Now get some more debug info
            var debugInfo = _queryInfoLazy.Value.Init(outSource, Log);

            wrapLog(null);
            // ...and return the results
			return new QueryRunDto
			{
				Query = results, 
				Streams = debugInfo.Streams,
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
		public bool Delete(int appId, int id) => _appManager.Queries.Delete(id);


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
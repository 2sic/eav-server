using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.Helpers;

namespace ToSic.Eav.WebApi
{
	/// <inheritdoc />
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public class QueryApi : HasLog
    {
        private readonly Lazy<AppManager> _appManagerLazy;
        private AppManager _appManager;

        public QueryApi(Lazy<AppManager> appManagerLazy): base("Api.EavQry")
        {
            _appManagerLazy = appManagerLazy;
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

            var qDef = _appManager.Read.Queries.Get(id.Value);

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
		    var wirings = JsonConvert.DeserializeObject<List<Connection>>(data.Pipeline[Constants.QueryStreamWiringAttributeName].ToString());

            _appManager.Queries.Update(id, data.DataSources, newDsGuids, data.Pipeline, wirings);

		    return Definition(appId, id);
		}


		/// <summary>
		/// Query the Result of a Pipeline using Test-Parameters
		/// </summary>
		public QueryRunDto Run(int appId, int id, int instanceId, LookUpEngine config)
		{
            var wrapLog = Log.Call($"a#{appId}, {nameof(id)}:{id}, {nameof(instanceId)}: {instanceId}");

            // Get the query, run it and track how much time this took
		    var queryFactory = new QueryBuilder(Log);
		    var qDef = queryFactory.GetQueryDefinition(appId, id);
			var outStreams = queryFactory.GetDataSourceForTesting(qDef, true, config);
            
            
            var serializeWrap = Log.Call("Serialize", useTimer: true);
            var timer = new Stopwatch();
            timer.Start();
		    var query = Helpers.Serializers.GetSerializerWithGuidEnabled().Convert(outStreams);
            timer.Stop();
            serializeWrap("ok");

            // Now get some more debug info
            var debugInfo = new DataSources.Debug.QueryInfo(outStreams, Log);

            wrapLog(null);
            // ...and return the results
			return new QueryRunDto
			{
				Query = query, 
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

                var deser = new Eav.ImportExport.Json.JsonSerializer(_appManager.AppState, Log);
                var ents = deser.Deserialize(args.GetContentString());
                var qdef = new QueryDefinition(ents, args.AppId, Log);
                _appManager.Queries.SaveCopy(qdef);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't import - probably bad file format", ex);
            }
        }

    }
}
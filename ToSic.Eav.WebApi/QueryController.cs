using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;

using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi
{
	/// <inheritdoc />
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public class QueryController : HasLog
    {
        #region constructors
        public QueryController(ILog parentLog): base("Api.EaPipe", parentLog)
		{
		}

        #endregion

        /// <summary>
        /// Get a Pipeline with DataSources
        /// </summary>
		public QueryDefinitionInfo GetPipeline(int appId, int? id = null)
        {
            Log.Add($"get pipe a#{appId}, id:{id}");
            var query = new QueryDefinitionInfo();
            var appManager = new AppManager(appId, Log);

            if (!id.HasValue) return query;

            var qDef = appManager.Read.Queries.Get(id.Value);

            #region Deserialize some Entity-Values

            query.Pipeline = qDef.Entity.AsDictionary();
            query.Pipeline[Constants.QueryStreamWiringAttributeName] = qDef.Connections;

            foreach (var part in qDef.Parts) 
                query.DataSources.Add(part.AsDictionary());

            #endregion

            return query;
        }


        /// <summary>
        /// Get installed DataSources from .NET Runtime but only those with [PipelineDesigner Attribute]
        /// </summary>
		public static IEnumerable<QueryRuntime.DataSourceInfo> GetInstalledDataSources()
		    => QueryRuntime.GetInstalledDataSources();

		/// <summary>
		/// Save Pipeline
		/// </summary>
		/// <param name="data">JSON object { pipeline: pipeline, dataSources: dataSources }</param>
		/// <param name="appId">AppId this Pipeline belongs to</param>
		/// <param name="id">PipelineEntityId</param>
		public QueryDefinitionInfo SavePipeline(QueryDefinitionInfo data, int appId, int id)
		{
		    Log.Add($"save pipe: a#{appId}, id#{id}");

            // assemble list of all new data-source guids, for later re-mapping when saving
            var newDsGuids = data.DataSources.Select(d => (string)d["EntityGuid"])
                .Where(g => g != "Out" && !g.StartsWith("unsaved"))
                .Select(Guid.Parse)
                .ToList();

            // Update Pipeline Entity with new Wirings etc.
		    var wirings = JsonConvert.DeserializeObject<List<Connection>>(data.Pipeline[Constants.QueryStreamWiringAttributeName].ToString());

            new AppManager(appId, Log).Queries.Update(id, data.DataSources, newDsGuids, data.Pipeline, wirings);

		    return GetPipeline(appId, id);
		}


		/// <summary>
		/// Query the Result of a Pipeline using Test-Parameters
		/// </summary>
		public dynamic QueryPipeline(int appId, int id, ILookUpEngine config)
		{
            var wrapLog = Log.Call($"a#{appId}, id:{id}");
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
			return new
			{
				Query = query, 
				Streams = debugInfo.Streams,
				Sources = debugInfo.Sources,
                QueryTimer = new { 
                    Milliseconds = timer.ElapsedMilliseconds,
                    Ticks = timer.ElapsedTicks
                }
			};
		}


        /// <summary>
        /// Clone a Pipeline with all DataSources and their configurations
        /// </summary>
        public void ClonePipeline(int appId, int id)
            => new AppManager(appId, Log).Queries.SaveCopy(id);
		

		/// <summary>
		/// Delete a Pipeline with the Pipeline Entity, Pipeline Parts and their Configurations. Stops if the if the Pipeline Entity has relationships to other Entities.
		/// </summary>
		public object DeletePipeline(int appId, int id)
		{
		    new AppManager(appId, Log).Queries.Delete(id);
			return new { Result = "Success" };
		}



        public bool ImportQuery(EntityImport args)
        {
            try
            {
                Log.Add("import content" + args.DebugInfo);
                var appManager = new AppManager(args.AppId, Log);

                var deser = new Eav.ImportExport.Json.JsonSerializer(appManager.AppState, Log);
                var ents = deser.Deserialize(args.GetContentString());
                var qdef = new QueryDefinition(ents, args.AppId, Log);
                appManager.Queries.SaveCopy(qdef);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't import - probably bad file format", ex);
            }
        }

    }
}
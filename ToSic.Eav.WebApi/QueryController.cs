using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Pipeline;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.ValueProvider;
using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi
{
	/// <inheritdoc />
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public class QueryController : HasLog
    {
        #region initializers etc. - work on later
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

            if (id.HasValue)
			{
			    var qdef = appManager.Read.Queries.Get(id.Value);

                #region Deserialize some Entity-Values
                query.Pipeline = EntityToDictionary(qdef.Header);
			    query.Pipeline[Constants.QueryStreamWiringAttributeName] = QueryWiring
                    .Deserialize((string)query.Pipeline[Constants.QueryStreamWiringAttributeName]);

				foreach (var part in qdef.Parts.Select(EntityToDictionary))
				{
					part[QueryConstants.VisualDesignerData] = JsonConvert
                        .DeserializeObject((string)part[QueryConstants.VisualDesignerData] ?? "");
					
                    // Replace ToSic.Eav with ToSic.Eav.DataSources because they moved to a different DLL
				    part[QueryConstants.PartAssemblyAndType] 
                        = QueryFactory.RewriteOldAssemblyNames((string)part[QueryConstants.PartAssemblyAndType]);
                    query.DataSources.Add(part);
				}
				#endregion
			}

            return query;

            Dictionary<string, object> EntityToDictionary(Interfaces.IEntity entity)
            {
                var attributes = entity.Attributes.ToDictionary(k => k.Value.Name, v =>  v.Value[0]);
                attributes.Add("EntityId", entity.EntityId);
                attributes.Add("EntityGuid", entity.EntityGuid);
                return attributes;
            }
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
		/// <param name="appId">AppId this Pipeline belogs to</param>
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
		    var wirings = JsonConvert.DeserializeObject<List<WireInfo>>(data.Pipeline[Constants.QueryStreamWiringAttributeName].ToString());

            new AppManager(appId, Log).Queries.Update(id, data.DataSources, newDsGuids, data.Pipeline, wirings);

		    return GetPipeline(appId, id);
		}


		/// <summary>
		/// Query the Result of a Pipline using Test-Parameters
		/// </summary>
		public dynamic QueryPipeline(int appId, int id, ValueCollectionProvider config)
		{
		    Log.Add($"queryy pipe: a#{appId}, id:{id}");
            // Get the query, run it and track how much time this took
		    var queryFactory = new QueryFactory(Log);
		    var qDef = queryFactory.GetQueryDefinition(appId, id);
			var outStreams = queryFactory.GetDataSourceForTesting(qDef, true, config);// ConstructPipeline(appId, id, true, config);
            var timer = new Stopwatch();
            timer.Start();
		    var query = Helpers.Serializers.GetSerializerWithGuidEnabled().Prepare(outStreams);
            timer.Stop();

            // Now get some more debug info
		    var debugInfo = new DataSources.Debug.QueryInfo(outStreams);

            // ...and return the results
			return new
			{
				Query = query, 
				debugInfo.Streams,
				debugInfo.Sources,
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

                var deser = new Eav.ImportExport.Json.JsonSerializer(appManager.Package, Log);
                var ents = deser.Deserialize(args.GetContentString());
                var qdef = new QueryDefinition(ents);
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
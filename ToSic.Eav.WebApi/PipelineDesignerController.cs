using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Pipeline;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi
{
	/// <inheritdoc />
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public class PipelineDesignerController : Eav3WebApiBase
    {
        #region initializers etc. - work on later
        public PipelineDesignerController(Log parentLog): base(parentLog)
		{
            Log.Rename("Api.EaPipe");
		}

        #endregion

        /// <summary>
        /// Get a Pipeline with DataSources
        /// </summary>
        [HttpGet]
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
			    query.Pipeline[Constants.DataPipelineStreamWiringStaticName] = DataPipelineWiring
                    .Deserialize((string)query.Pipeline[Constants.DataPipelineStreamWiringStaticName]);

				foreach (var part in qdef.Parts.Select(EntityToDictionary))
				{
					part[QueryConstants.VisualDesignerData] = JsonConvert
                        .DeserializeObject((string)part[QueryConstants.VisualDesignerData] ?? "");
					
                    // Replace ToSic.Eav with ToSic.Eav.DataSources because they moved to a different DLL
				    part[QueryConstants.PartAssemblyAndType] 
                        = DataPipelineFactory.RewriteOldAssemblyNames((string)part[QueryConstants.PartAssemblyAndType]);
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
        [HttpGet]
		public IEnumerable<QueryRuntime.DataSourceInfo> GetInstalledDataSources()
		    => QueryRuntime.GetInstalledDataSources();

		/// <summary>
		/// Save Pipeline
		/// </summary>
		/// <param name="data">JSON object { pipeline: pipeline, dataSources: dataSources }</param>
		/// <param name="appId">AppId this Pipeline belogs to</param>
		/// <param name="id">PipelineEntityId</param>
		public QueryDefinitionInfo SavePipeline([FromBody] QueryDefinitionInfo data, int appId, int id)
		{
		    Log.Add($"save pipe: a#{appId}, id#{id}");

            // assemble list of all new data-source guids, for later re-mapping when saving
            var newDsGuids = data.DataSources.Select(d => (string)d["EntityGuid"])
                .Where(g => g != "Out" && !g.StartsWith("unsaved"))
                .Select(Guid.Parse)
                .ToList();

            // Update Pipeline Entity with new Wirings etc.
		    var wirings = JsonConvert.DeserializeObject<List<WireInfo>>(data.Pipeline[Constants.DataPipelineStreamWiringStaticName].ToString());

            new AppManager(appId, Log).Queries.Update(id, data.DataSources, newDsGuids, data.Pipeline, wirings);

		    return GetPipeline(appId, id);
		}


		/// <summary>
		/// Query the Result of a Pipline using Test-Parameters
		/// </summary>
		[HttpGet]
		public dynamic QueryPipeline(int appId, int id)
		{
		    Log.Add($"queryy pipe: a#{appId}, id:{id}");
            // Get the query, run it and track how much time this took
			var outStreams = ConstructPipeline(appId, id, true);
		    var timer = new Stopwatch();
            timer.Start();
		    var query = Serializer.Prepare(outStreams);
            timer.Stop();

            // Now get some more debug info
		    var debugInfo = new DataSources.Debug.PipelineInfo(outStreams);

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


		private IDataSource ConstructPipeline(int appId, int id, bool showDrafts) 
            => new DataPipelineFactory(Log).GetDataSourceForTesting(appId, id, showDrafts);

        [HttpGet]
        public dynamic PipelineDebugInfo(int appId, int id)
            => new DataSources.Debug.PipelineInfo(ConstructPipeline(appId, id, true));


        /// <summary>
        /// Clone a Pipeline with all DataSources and their configurations
        /// </summary>
        [HttpGet]
        public void ClonePipeline(int appId, int id)
            => new AppManager(appId, Log).Queries.SaveCopy(id);
		

		/// <summary>
		/// Delete a Pipeline with the Pipeline Entity, Pipeline Parts and their Configurations. Stops if the if the Pipeline Entity has relationships to other Entities.
		/// </summary>
		[HttpGet]
		public object DeletePipeline(int appId, int id)
		{
		    new AppManager(appId, Log).Queries.Delete(id);
			return new { Result = "Success" };
		}



        [HttpPost]
        public bool ImportQuery(EntityImport args)
        {
            try
            {
                Log.Add("import content" + args.DebugInfo);
                AppId = args.AppId;

                //var data = Convert.FromBase64String(args.ContentBase64);
                //var str = Encoding.UTF8.GetString(data);

                var deser = new ImportExport.Json.JsonSerializer(AppManager.Package, Log);
                var ents = deser.Deserialize(args.GetContentString());
                var qdef = new QueryDefinition(ents);
                AppManager.Queries.SaveCopy(qdef);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't import - probably bad file format", ex);
            }
        }

    }
}
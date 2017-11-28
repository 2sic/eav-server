﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Pipeline;
using ToSic.Eav.Logging.Simple;

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
		public Dictionary<string, object> GetPipeline(int appId, int? id = null)
        {
            Log.Add($"get pipe a#{appId}, id:{id}");
			Dictionary<string, object> pipelineJson = null;
			var dataSourcesJson = new List<Dictionary<string, object>>();
            var appManager = new AppManager(appId, Log);

            if (id.HasValue)
			{
			    var qdef = appManager.Read.Queries.Get(id.Value);

                #region Deserialize some Entity-Values
                pipelineJson = EntityToDictionary(qdef.Header);
				pipelineJson[Constants.DataPipelineStreamWiringStaticName] = DataPipelineWiring
                    .Deserialize((string)pipelineJson[Constants.DataPipelineStreamWiringStaticName]);

				foreach (var dataSource in qdef.Parts.Select(EntityToDictionary))
				{
					dataSource[QueryConstants.VisualDesignerData] = JsonConvert
                        .DeserializeObject((string)dataSource[QueryConstants.VisualDesignerData] ?? "");
					
                    // Replace ToSic.Eav with ToSic.Eav.DataSources because they moved to a different DLL
				    dataSource[QueryConstants.PartAssemblyAndType] 
                        = DataPipelineFactory.RewriteOldAssemblyNames((string)dataSource[QueryConstants.PartAssemblyAndType]);
                    dataSourcesJson.Add(dataSource);
				}
				#endregion
			}

			// return consolidated Data
			return new Dictionary<string, object>
			{
				{"Pipeline", pipelineJson},
				{"DataSources", dataSourcesJson}
			};

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
		public Dictionary<string, object> SavePipeline([FromBody] JToken data, int appId, int id)
		{
		    Log.Add($"save pipe: a#{appId}, id#{id}");

            // extract typed structure of the data, for further processing
		    var headerValues = ValuesDictionary(data["pipeline"]);
		    var partsDics = data["dataSources"].ToObject<List<Dictionary<string, object>>>();

            // assemble list of all new data-source guids, for later re-mapping when saving
            var newDsGuids = data["dataSources"]
                .Select(d => (string)d["EntityGuid"])
                .Where(g => g != "Out" && !g.StartsWith("unsaved"))
                .Select(Guid.Parse)
                .ToList();

            // Update Pipeline Entity with new Wirings etc.
		    var wirings = data["pipeline"][Constants.DataPipelineStreamWiringStaticName].ToObject<List<WireInfo>>();

            new AppManager(appId, Log).Queries.Update(id, partsDics, newDsGuids, headerValues, wirings);

		    return GetPipeline(appId, id);
		}
        

        /// <summary>
		/// Update an Entity with values from a JObject
		/// </summary>
		/// <param name="newValues">JObject with new Values</param>
		private static Dictionary<string, object> ValuesDictionary(JToken newValues)
		{
            var excludeKeysStatic = new[] { Constants.EntityFieldGuid, Constants.EntityFieldId };
			return newValues.ToObject<IDictionary<string, object>>()
                .Where(i => !excludeKeysStatic.Contains(i.Key, StringComparer.InvariantCultureIgnoreCase))
                .ToDictionary(k => k.Key, v => v.Value);
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
		    var debugInfo = new DataSources.Debug.PipelineInfo(outStreams);// ConstructPipeline(appId, id));

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
		{
		    Log.Add($"construct pipe a#{appId}, pipe:{id}, drafts:{showDrafts}");
			var testValueProviders = new DataPipelineFactory(Log).GetTestValueProviders(appId, id).ToList();
		    return new DataPipelineFactory(Log).GetDataSource(appId, id, testValueProviders, showDrafts:showDrafts);
		}

        [HttpGet]
        public dynamic PipelineDebugInfo(int appId, int id)
            => new DataSources.Debug.PipelineInfo(ConstructPipeline(appId, id, true));


        /// <summary>
        /// Clone a Pipeline with all DataSources and their configurations
        /// </summary>
        [HttpGet]
        public void ClonePipeline(int appId, int id)
            => new AppManager(appId, Log).Queries.Clone(id);
		

		/// <summary>
		/// Delete a Pipeline with the Pipeline Entity, Pipeline Parts and their Configurations. Stops if the if the Pipeline Entity has relationships to other Entities.
		/// </summary>
		[HttpGet]
		public object DeletePipeline(int appId, int id)
		{
		    Log.Add($"delete a#{appId}, id:{id}");
		    new AppManager(appId, Log).Queries.Delete(id);
			return new { Result = "Success" };
		}
	}
}
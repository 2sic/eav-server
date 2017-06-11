using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
//using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.Serializers;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.WebApi
{
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public class PipelineDesignerController : ApiController
    {
        #region initializers etc. - work on later
        #region Helpers
        // I must keep the serializer so it can be configured from outside if necessary
        private Serializer _serializer;
        public Serializer Serializer => _serializer ?? (_serializer = Factory.Resolve<Serializer>());

	    #endregion
		public List<IValueProvider> ValueProviders { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eavConnectionString">optional EAV Connection String</param>
        public PipelineDesignerController(/*string userName,*/ string eavConnectionString = null)
		{
			// Init empty list of ValueProviders
			ValueProviders = new List<IValueProvider>();
		}

        #endregion

        /// <summary>
        /// Get a Pipeline with DataSources
        /// </summary>
        [HttpGet]
		public Dictionary<string, object> GetPipeline(int appId, int? id = null)
		{
			Dictionary<string, object> pipelineJson = null;
			var dataSourcesJson = new List<Dictionary<string, object>>();

			if (id.HasValue)
			{
				// Get the Entity describing the Pipeline and Pipeline Parts (DataSources)
				var source = DataSource.GetInitialDataSource(appId: appId);
				var pipelineEntity = DataPipeline.GetPipelineEntity(id.Value, source);
				var dataSources = DataPipeline.GetPipelineParts(source.ZoneId, source.AppId, pipelineEntity.EntityGuid);

				#region Deserialize some Entity-Values
				pipelineJson = EntityToDictionary(pipelineEntity);
				pipelineJson[Constants.DataPipelineStreamWiringStaticName] = DataPipelineWiring.Deserialize((string)pipelineJson[Constants.DataPipelineStreamWiringStaticName]);

				foreach (var dataSource in dataSources.Select(e => EntityToDictionary(e)))// Helpers.GetEntityValues(dataSources))
				{
					dataSource["VisualDesignerData"] = JsonConvert.DeserializeObject((string)dataSource["VisualDesignerData"]);
					
                    // Replace ToSic.Eav with ToSic.Eav.DataSources because they moved to a different DLL
                    // Using a trick to add a special EOL-character to ensure we don't replace somethin in the middle
					var partAssemblyAndType = (string)dataSource["PartAssemblyAndType"];
                    if(partAssemblyAndType.EndsWith(Constants.V3To4DataSourceDllOld)) // only do this replace if really necessary to prevent side effects
                        dataSource["PartAssemblyAndType"] = partAssemblyAndType.Replace(Constants.V3To4DataSourceDllOld, Constants.V3To4DataSourceDllNew);
                    dataSourcesJson.Add(dataSource);
				}
				#endregion
			}

            Tuple<Dictionary<string, object>, List<Dictionary<string, object>>> set = new Tuple<Dictionary<string, object>, List<Dictionary<string, object>>>(pipelineJson, dataSourcesJson);
		    


			// return consolidated Data
			return new Dictionary<string, object>
			{
				{"Pipeline", set.Item1},
				{"DataSources", set.Item2}
			};

            Dictionary<string, object> EntityToDictionary(IEntity entity)
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
		public IEnumerable<QueryRuntime.QueryInfoTemp> GetInstalledDataSources()
		    => QueryRuntime.GetInstalledDataSources();

		/// <summary>
		/// Save Pipeline
		/// </summary>
		/// <param name="data">JSON object { pipeline: pipeline, dataSources: dataSources }</param>
		/// <param name="appId">AppId this Pipeline belogs to</param>
		/// <param name="id">PipelineEntityId</param>
		public Dictionary<string, object> SavePipeline([FromBody] dynamic data, int appId, int? id = null)
		{
            var appManager = new AppManager(appId);

			//_context.UserName = _userName;
			var source = DataSource.GetInitialDataSource(appId: appId);

			// Get/Save Pipeline EntityGuid. Its required to assign Pipeline Parts to it.
			Guid pipelineEntityGuid;
			if (id.HasValue)
			{
				var entity = DataPipeline.GetPipelineEntity(id.Value, source);
				pipelineEntityGuid = entity.EntityGuid;

				if (((IAttribute<bool?>)entity["AllowEdit"]).TypedContents == false)
					throw new InvalidOperationException("Pipeline has AllowEdit set to false");
			}
			else
			{
				throw new NotSupportedException();
				//Entity entity = SavePipelineEntity(null, data.pipeline);
				//pipelineEntityGuid = entity.EntityGUID;
				//id = entity.EntityID;
			}

		    var pipelinePartAttributeSetId = appManager.Read.ContentTypes.Get(Constants.DataPipelinePartStaticName).StaticName;
			var newDataSources = SavePipelineParts(data.dataSources, pipelineEntityGuid, pipelinePartAttributeSetId, appManager);
			DeletedRemovedPipelineParts(data.dataSources, newDataSources, pipelineEntityGuid, source.ZoneId, source.AppId, appManager);

			// Update Pipeline Entity with new Wirings etc.
		    SavePipelineEntity(id.Value, appId, data.pipeline, newDataSources, appManager);

			return GetPipeline(appId, id.Value);
		}

        /// <summary>
        /// Save PipelineParts (DataSources) to EAV
        /// </summary>
        /// <param name="dataSources">JSON describing the DataSources</param>
        /// <param name="pipelineEntityGuid">EngityGuid of the Pipeline-Entity</param>
        /// <param name="pipelinePartAttributeSetId">AttributeSetId of PipelineParts</param>
        /// <param name="appManager"></param>
        private Dictionary<string, Guid> SavePipelineParts(dynamic dataSources, Guid pipelineEntityGuid, /*int*/string pipelinePartAttributeSetId, AppManager appManager)//, DbDataController _context)
		{
			var newDataSources = new Dictionary<string, Guid>();

			foreach (var dataSource in dataSources)
			{
				// Skip Out-DataSource
				if (dataSource.EntityGuid == "Out") continue;

				// Update existing DataSource
				var newValues = GetEntityValues(dataSource);
				if (dataSource.EntityId != null)
                    appManager.Entities.Update((int)dataSource.EntityId, newValues);
				// Add new DataSource
				else
				{

                    Tuple<int, Guid> entity = appManager.Entities.Create(pipelinePartAttributeSetId, newValues, new Metadata { TargetType = Constants.MetadataForEntity, KeyGuid = pipelineEntityGuid});
                    newDataSources.Add((string)dataSource.EntityGuid, entity.Item2);
				}
			}

			return newDataSources;
		}

		/// <summary>
		/// Delete Pipeline Parts (DataSources) that are not present
		/// </summary>
		private void DeletedRemovedPipelineParts(IEnumerable<JToken> dataSources, Dictionary<string, Guid> newDataSources, Guid pipelineEntityGuid, int zoneId, int appId, AppManager appManager)//, DbDataController _context)
		{
			// Get EntityGuids currently stored in EAV
			var existingEntityGuids = DataPipeline.GetPipelineParts(zoneId, appId, pipelineEntityGuid).Select(e => e.EntityGuid);

			// Get EntityGuids from the UI (except Out and unsaved)
			var newEntityGuids = dataSources.Select(d => (string)((JObject)d).Property("EntityGuid").Value).Where(g => g != "Out" && !g.StartsWith("unsaved")).Select(Guid.Parse).ToList();
			newEntityGuids.AddRange(newDataSources.Values);

		    foreach (var entityToDelete in existingEntityGuids.Where(existingGuid => !newEntityGuids.Contains(existingGuid)))
		        appManager.Entities.Delete(entityToDelete);
		}

        /// <summary>
        /// Save a Pipeline Entity to EAV
        /// </summary>
        /// <param name="id">EntityId of the Entity describing the Pipeline</param>
        /// <param name="appId"></param>
        /// <param name="pipeline">JSON with the new Entity-Values</param>
        /// <param name="newDataSources">Array with new DataSources and the unsavedName and final EntityGuid</param>
        /// <param name="appManager"></param>
        private void SavePipelineEntity(int? id, int appId, dynamic pipeline, IDictionary<string, Guid> newDataSources, AppManager appManager)//, DbDataController _context)
		{
            #region prevent save without pipeline ID
            if (!id.HasValue)
            {
                //var attributeSetId = _context.GetAttributeSet(DataSource.DataPipelineStaticName).AttributeSetID;
                // return _context.AddEntity(attributeSetId, newValues, null, null)
                throw new NotSupportedException("Saving a new Pipeline directly from the Pipeline-Designer is currently disabled because of Culture/Dimension Issues. Please create a new Pipeline using the Pipeline Management.");
            }
            #endregion

            // Create a clone so it can be modifie before saving but doesn't affect the underlaying JObject.
            // A new Pipeline Entity must be saved twice (create and update), but some Field-Values are changed before saving it
            dynamic pipelineClone = pipeline.DeepClone();

			// Update Wirings of Entities just added
			IEnumerable<WireInfo> wirings = pipeline.StreamWiring.ToObject<IEnumerable<WireInfo>>();
			if (newDataSources != null)
			{
				var wiringsNew = new List<WireInfo>();
				foreach (var wireInfo in wirings)
				{
					var newWireInfo = wireInfo;
					if (newDataSources.ContainsKey(wireInfo.From))
						newWireInfo.From = newDataSources[wireInfo.From].ToString();
					if (newDataSources.ContainsKey(wireInfo.To))
						newWireInfo.To = newDataSources[wireInfo.To].ToString();

					wiringsNew.Add(newWireInfo);
				}
				wirings = wiringsNew;
			}
			// Validate Stream Wirings
			foreach (var wireInfo in wirings.Where(wireInfo => wirings.Count(w => w.To == wireInfo.To && w.In == wireInfo.In) > 1))
				throw new Exception(
				    $"DataSource \"{wireInfo.To}\" has multiple In-Streams with Name \"{wireInfo.In}\". Each In-Stream must have an unique Name and can have only one connection.");


			pipelineClone.StreamWiring = DataPipelineWiring.Serialize(wirings);

			// Add/Update Entity
			var newValues = GetEntityValues(pipelineClone);


			// Guess DimensionIDs for the Pipeline-Entity
			var source = DataSource.GetInitialDataSource(appId: appId);
			var pipelineEntity = DataPipeline.GetPipelineEntity(id.Value, source);
			int[] dimensionIds = null;

			if (pipelineEntity.Title.Values.Any())
				dimensionIds = pipelineEntity.Title.Values.First().Languages.Select(l => l.DimensionId).ToArray();

            appManager.Entities.Update(id.Value, newValues, dimensionIds: dimensionIds);
		}

		/// <summary>
		/// Update an Entity with values from a JObject
		/// </summary>
		/// <param name="newValues">JObject with new Values</param>
		private static Dictionary<string, object> GetEntityValues(JToken newValues)
		{
			var newValuesDict = newValues.ToObject<IDictionary<string, object>>();

			var excludeKeysStatic = new[] { "EntityGuid", "EntityId" };

			return newValuesDict.Where(i => !excludeKeysStatic.Contains(i.Key)).ToDictionary(k => k.Key, v => v.Value);
		}


		/// <summary>
		/// Query the Result of a Pipline using Test-Parameters
		/// </summary>
		[HttpGet]
		public dynamic QueryPipeline(int appId, int id)
		{
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


		private static IDataSource ConstructPipeline(int appId, int id, bool showDrafts)
		{
			var testValueProviders = DataPipelineFactory.GetTestValueProviders(appId, id);
			return DataPipelineFactory.GetDataSource(appId, id, testValueProviders, showDrafts:showDrafts);
		}

		[HttpGet]
		public dynamic PipelineDebugInfo(int appId, int id)
		=> new DataSources.Debug.PipelineInfo(ConstructPipeline(appId, id, true));


        /// <summary>
        /// Clone a Pipeline with all DataSources and their configurations
        /// </summary>
        [HttpGet]
        public object ClonePipeline(int appId, int id)
            => new { EntityId = new AppManager(appId).Queries.Clone(id) };
		

		/// <summary>
		/// Delete a Pipeline with the Pipeline Entity, Pipeline Parts and their Configurations. Stops if the if the Pipeline Entity has relationships to other Entities.
		/// </summary>
		[HttpGet]
		public object DeletePipeline(int appId, int id)
		{
		    new AppManager(appId).Queries.Delete(id);
			return new { Result = "Success" };
		}
	}
}
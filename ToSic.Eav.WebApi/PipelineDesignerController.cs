using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToSic.Eav.BLL;
using ToSic.Eav.BLL.Parts;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Serializers;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.WebApi
{
	/// <summary>
	/// Web API Controller for the Pipeline Designer UI
	/// </summary>
	public class PipelineDesignerController : ApiController
    {
        #region Helpers
        // I must keep the serializer so it can be configured from outside if necessary
        private Serializer _serializer;
        public Serializer Serializer => _serializer ?? (_serializer = Factory.Container.Resolve<Serializer>());

	    #endregion
        //private EavDataController _context;
		private readonly string _userName;
		public List<IValueProvider> ValueProviders { get; set; }

		/// <summary>
		/// Default Constructor
		/// </summary>
		public PipelineDesignerController() : this("EAV Pipeline Designer") { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="userName">current UserName</param>
		/// <param name="eavConnectionString">optional EAV Connection String</param>
		public PipelineDesignerController(string userName, string eavConnectionString = null)
		{
            // TODO URGENT - TRYING TO GET THIS TO WORK - HAS SIDE EFFECTS ON OTHER SERVICES
            // Must discuss w/2bg, seems to work now anyhow...
			// Preserving circular reference
			// GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
			// GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

			_userName = userName;

			if (eavConnectionString != null)
				Eav.Configuration.SetConnectionString(eavConnectionString);

			// Init empty list of ValueProviders
			ValueProviders = new List<IValueProvider>();
		}


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
				pipelineJson = Helpers.GetEntityValues(pipelineEntity);
				pipelineJson[Constants.DataPipelineStreamWiringStaticName] = DataPipelineWiring.Deserialize((string)pipelineJson[Constants.DataPipelineStreamWiringStaticName]);

				foreach (var dataSource in Helpers.GetEntityValues(dataSources))
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

			// return consolidated Data
			return new Dictionary<string, object>
			{
				{"Pipeline", pipelineJson},
				{"DataSources", dataSourcesJson}
			};
		}

		/// <summary>
		/// Get installed DataSources from .NET Runtime but only those with [PipelineDesigner Attribute]
		/// </summary>
		[HttpGet]
		public IEnumerable<object> GetInstalledDataSources()
		{
			var result = new List<object>();
			var installedDataSources = DataSource.GetInstalledDataSources();
			foreach (var dataSource in installedDataSources.Where(d => d.GetCustomAttributes(typeof(PipelineDesignerAttribute), false).Any()))
			{
				#region Create Instance of DataSource to get In- and Out-Streams
				ICollection<string> outStreamNames = new string[0];
				ICollection<string> inStreamNames = new string[0];
				if (!dataSource.IsInterface && !dataSource.IsAbstract)
				{
					var dataSourceInstance = (IDataSource)Activator.CreateInstance(dataSource);
					try
					{
						outStreamNames = dataSourceInstance.Out.Keys;
					}
					catch
					{
						outStreamNames = null;
					}
				}
				// Handle Interfaces (currently only ICache) with Unity
				else if (dataSource.IsInterface)
				{
					var dataSourceInstance = (IDataSource)Factory.Container.Resolve(dataSource);
					outStreamNames = dataSourceInstance.Out.Keys;
					if (dataSourceInstance is ICache)
						inStreamNames = null;
				}
				#endregion

				result.Add(new
				{
					PartAssemblyAndType = dataSource.FullName + ", " + dataSource.Assembly.GetName().Name,
					ClassName = dataSource.Name,
					In = inStreamNames,
					Out = outStreamNames
				});
			}

			return result;
		}

		/// <summary>
		/// Save Pipeline
		/// </summary>
		/// <param name="data">JSON object { pipeline: pipeline, dataSources: dataSources }</param>
		/// <param name="appId">AppId this Pipeline belogs to</param>
		/// <param name="id">PipelineEntityId</param>
		public Dictionary<string, object> SavePipeline([FromBody] dynamic data, int appId, int? id = null)
		{
			var _context = EavDataController.Instance(appId: appId);
			_context.UserName = _userName;
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

			var pipelinePartAttributeSetId = _context.AttribSet.GetAttributeSet(Constants.DataPipelinePartStaticName).AttributeSetID;
			var newDataSources = SavePipelineParts(data.dataSources, pipelineEntityGuid, pipelinePartAttributeSetId, _context);
			DeletedRemovedPipelineParts(data.dataSources, newDataSources, pipelineEntityGuid, source.ZoneId, source.AppId, _context);

			// Update Pipeline Entity with new Wirings etc.
			SavePipelineEntity(id.Value, appId, data.pipeline, newDataSources, _context);

			return GetPipeline(appId, id.Value);
		}

		/// <summary>
		/// Save PipelineParts (DataSources) to EAV
		/// </summary>
		/// <param name="dataSources">JSON describing the DataSources</param>
		/// <param name="pipelineEntityGuid">EngityGuid of the Pipeline-Entity</param>
		/// <param name="pipelinePartAttributeSetId">AttributeSetId of PipelineParts</param>
		private Dictionary<string, Guid> SavePipelineParts(dynamic dataSources, Guid pipelineEntityGuid, int pipelinePartAttributeSetId, EavDataController _context)
		{
			var newDataSources = new Dictionary<string, Guid>();

			foreach (var dataSource in dataSources)
			{
				// Skip Out-DataSource
				if (dataSource.EntityGuid == "Out") continue;

				// Update existing DataSource
				var newValues = GetEntityValues(dataSource);
				if (dataSource.EntityId != null)
					_context.Entities.UpdateEntity((int)dataSource.EntityId, newValues);
				// Add new DataSource
				else
				{
					var entitiy = _context.Entities.AddEntity(pipelinePartAttributeSetId, newValues, null, pipelineEntityGuid, Constants.AssignmentObjectTypeEntity);
					newDataSources.Add((string)dataSource.EntityGuid, entitiy.EntityGUID);
				}
			}

			return newDataSources;
		}

		/// <summary>
		/// Delete Pipeline Parts (DataSources) that are not present
		/// </summary>
		private void DeletedRemovedPipelineParts(IEnumerable<JToken> dataSources, Dictionary<string, Guid> newDataSources, Guid pipelineEntityGuid, int zoneId, int appId, EavDataController _context)
		{
			// Get EntityGuids currently stored in EAV
			var existingEntityGuids = DataPipeline.GetPipelineParts(zoneId, appId, pipelineEntityGuid).Select(e => e.EntityGuid);

			// Get EntityGuids from the UI (except Out and unsaved)
			var newEntityGuids = dataSources.Select(d => (string)((JObject)d).Property("EntityGuid").Value).Where(g => g != "Out" && !g.StartsWith("unsaved")).Select(Guid.Parse).ToList();
			newEntityGuids.AddRange(newDataSources.Values);

			foreach (var entityToDelet in existingEntityGuids.Where(existingGuid => !newEntityGuids.Contains(existingGuid)))
				_context.Entities.DeleteEntity(entityToDelet);
		}

	    /// <summary>
	    /// Save a Pipeline Entity to EAV
	    /// </summary>
	    /// <param name="id">EntityId of the Entity describing the Pipeline</param>
	    /// <param name="appId"></param>
	    /// <param name="pipeline">JSON with the new Entity-Values</param>
	    /// <param name="newDataSources">Array with new DataSources and the unsavedName and final EntityGuid</param>
	    private void SavePipelineEntity(int? id, int appId, dynamic pipeline, IDictionary<string, Guid> newDataSources, EavDataController _context)
		{
			// Create a clone so it can be modifie before saving but doesn't affect the underlaying JObject.
			// A new Pipeline Entity must be saved twice, but some Field-Values are changed before saving it
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
			pipelineClone.StreamWiring = DataPipelineWiring.Serialize(wirings);

			// Validate Stream Wirings
			foreach (var wireInfo in wirings.Where(wireInfo => wirings.Count(w => w.To == wireInfo.To && w.In == wireInfo.In) > 1))
				throw new Exception(
				    $"DataSource \"{wireInfo.To}\" has multiple In-Streams with Name \"{wireInfo.In}\". Each In-Stream must have an unique Name and can have only one connection.");

			// Add/Update Entity
			IDictionary newValues = GetEntityValues(pipelineClone);

			if (!id.HasValue)
			{
				//var attributeSetId = _context.GetAttributeSet(DataSource.DataPipelineStaticName).AttributeSetID;
				// return _context.AddEntity(attributeSetId, newValues, null, null)
				throw new NotSupportedException("Saving a new Pipeline directly from the Pipeline-Designer is currently disabled because of Culture/Dimension Issues. Please create a new Pipeline using the Pipeline Management.");
			}

			// Guess DimensionIDs for the Pipeline-Entity
			var source = DataSource.GetInitialDataSource(appId: appId);
			var pipelineEntity = DataPipeline.GetPipelineEntity(id.Value, source);
			int[] dimensionIds = null;

			if (pipelineEntity.Title.Values.Any())
				dimensionIds = pipelineEntity.Title.Values.First().Languages.Select(l => l.DimensionId).ToArray();

	        _context.Entities.UpdateEntity(id.Value, newValues, dimensionIds: dimensionIds);
		}

		/// <summary>
		/// Update an Entity with values from a JObject
		/// </summary>
		/// <param name="newValues">JObject with new Values</param>
		private static IDictionary GetEntityValues(JToken newValues)
		{
			var newValuesDict = newValues.ToObject<IDictionary<string, object>>();

			var excludeKeysStatic = new[] { "EntityGuid", "EntityId" };

			return newValuesDict.Where(i => !excludeKeysStatic.Contains(i.Key)).ToDictionary(k => k.Key, v => v.Value);
		}


		/// <summary>
		/// Query the Result of a Pipline using Test-Parameters
		/// </summary>
		[HttpGet]
		public dynamic /* Dictionary<string, IEnumerable<IEntity>> */ QueryPipeline(int appId, int id)
		{
            // Get the query, run it and track how much time this took
			var outStreams = ConstructPipeline(appId, id);
		    var timer = new Stopwatch();
            timer.Start();
		    var query = Serializer.Prepare(outStreams);
            timer.Stop();

            // Now get some more debug info
		    var debugInfo = new DataSources.Debug.PipelineInfo(outStreams);// ConstructPipeline(appId, id));

            // ...and return the results
			return new
			{
				Query = query, // Serializer.Prepare(outStreams), // outStreams.Out.ToDictionary(k => k.Key, v => v.Value.List.Select(l => l.Value)),
				debugInfo.Streams,
				debugInfo.Sources,
                QueryTimer = new { 
                    Milliseconds = timer.ElapsedMilliseconds,
                    Ticks = timer.ElapsedTicks
                }
			};
		}


		private static IDataSource ConstructPipeline(int appId, int id)
		{
			var testValueProviders = DataPipelineFactory.GetTestValueProviders(appId, id);
			return DataPipelineFactory.GetDataSource(appId, id, testValueProviders);
		}

		[HttpGet]
		public dynamic PipelineDebugInfo(int appId, int id)
		=> new DataSources.Debug.PipelineInfo(ConstructPipeline(appId, id));
		





		/// <summary>
		/// Clone a Pipeline with all DataSources and their configurations
		/// </summary>
		[HttpGet]
		public object ClonePipeline(int appId, int id)
		{
            var eavCt = EavDataController.Instance(appId: appId);
            var clonePipelineEntity = new DbPipeline(eavCt).CopyDataPipeline(appId, id, _userName);
			return new { EntityId = clonePipelineEntity.EntityID };
		}

		/// <summary>
		/// Delete a Pipeline with the Pipeline Entity, Pipeline Parts and their Configurations. Stops if the if the Pipeline Entity has relationships to other Entities.
		/// </summary>
		[HttpGet]
		public object DeletePipeline(int appId, int id)
		{
			//if (_context == null)
				var _context = EavDataController.Instance(appId: appId);

		    var canDeleteResult = (_context.Entities.CanDeleteEntity(id));// _context.EntCommands.CanDeleteEntity(id);
			if (!canDeleteResult.Item1)
				throw new Exception(canDeleteResult.Item2);


			// Get the Entity describing the Pipeline and Pipeline Parts (DataSources)
			var source = DataSource.GetInitialDataSource(appId: appId);
			var pipelineEntity = DataPipeline.GetPipelineEntity(id, source);
			var dataSources = DataPipeline.GetPipelineParts(source.ZoneId, source.AppId, pipelineEntity.EntityGuid);
			var metaDataSource = DataSource.GetMetaDataSource(appId: appId);

			// Delete Pipeline Parts
			foreach (var dataSource in dataSources)
			{
				// Delete Configuration Entities (if any)
				var dataSourceConfig = metaDataSource.GetAssignedEntities(Constants.AssignmentObjectTypeEntity, dataSource.EntityGuid).FirstOrDefault();
				if (dataSourceConfig != null)
					_context.Entities.DeleteEntity(dataSourceConfig.EntityId);

				_context.Entities.DeleteEntity(dataSource.EntityId);
			}

			// Delete Pipeline
			_context.Entities.DeleteEntity(id);

			return new { Result = "Success" };
		}
	}
}
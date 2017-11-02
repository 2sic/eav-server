using System;
using System.Collections.Generic;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Helpers to work with Data Pipelines
	/// </summary>
	public class DataPipeline
	{
	    /// <summary>
		/// Get an Entity Describing a Pipeline
		/// </summary>
		/// <param name="entityId">EntityId</param>
		/// <param name="dataSource">DataSource to load Entity from</param>
		public static IEntity GetPipelineEntity(int entityId, IDataSource dataSource)
		{
			var entities = dataSource[Constants.DefaultStreamName].List;

			IEntity pipelineEntity;
			try
			{
				pipelineEntity = entities[entityId];
                if (pipelineEntity.Type.StaticName != Constants.DataPipelineStaticName)//PipelineAttributeSetStaticName)
					throw new ArgumentException("Entity is not an DataPipeline Entity", nameof(entityId));
			}
			catch (Exception)
			{
				throw new ArgumentException($"Could not load Pipeline-Entity with ID {entityId}.", nameof(entityId));
			}

			return pipelineEntity;
		}

		/// <summary>
		/// Get Entities Describing PipelineParts
		/// </summary>
		/// <param name="zoneId">zoneId of the Pipeline</param>
		/// <param name="appId">appId of the Pipeline</param>
		/// <param name="pipelineEntityGuid">EntityGuid of the Entity describing the Pipeline</param>
		public static IEnumerable<IEntity> GetPipelineParts(int zoneId, int appId, Guid pipelineEntityGuid)
		{
			var metaDataSource = DataSource.GetMetaDataSource(zoneId, appId);
			return metaDataSource.GetMetadata(Constants.MetadataForEntity, pipelineEntityGuid, Constants.DataPipelinePartStaticName);
		}


        /// <summary>
        /// Assembles a list of all queries / pipelines configured for this app. 
        /// The queries internally are not assembled yet for performance reasons...
        /// ...but will be auto-assembled the moment they are accessed
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <param name="valuesCollectionProvider"></param>
        /// <returns></returns>
	    public static Dictionary<string, IDataSource> AllPipelines(int zoneId, int appId, IValueCollectionProvider valuesCollectionProvider, Log parentLog)
	    {
            var source = DataSource.GetInitialDataSource(appId: appId, parentLog:parentLog);
            var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: source);
            typeFilter.TypeName = Constants.DataPipelineStaticName;

	        var dict = new Dictionary<string, IDataSource>(StringComparer.OrdinalIgnoreCase);
	        foreach (var entQuery in typeFilter.List)
	        {
	            var delayedQuery = new DeferredPipelineQuery(zoneId, appId, entQuery.Value, valuesCollectionProvider);
                dict.Add(entQuery.Value.Title[0].ToString(), delayedQuery);
	        }
	        return dict;
	    }

        
	}
}

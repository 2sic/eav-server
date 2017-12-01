using System;
using System.Collections.Generic;
using ToSic.Eav.Data.Query;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.DataSources.Pipeline
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
			try
			{
			    var pipelineEntity = dataSource.List.FindRepoId(entityId);
                if (pipelineEntity.Type.StaticName != Constants.DataPipelineStaticName)
                    throw new ArgumentException("Entity is not an DataPipeline Entity", nameof(entityId));
			    return pipelineEntity;
			}
			catch (Exception)
			{
				throw new ArgumentException($"Could not load Pipeline-Entity with ID {entityId}.", nameof(entityId));
			}

		}

        /// <summary>
        /// Assembles a list of all queries / pipelines configured for this app. 
        /// The queries internally are not assembled yet for performance reasons...
        /// ...but will be auto-assembled the moment they are accessed
        /// </summary>
        /// <returns></returns>
	    public static Dictionary<string, IDataSource> AllPipelines(int zoneId, int appId, IValueCollectionProvider valuesCollectionProvider, Log parentLog)
	    {
            var source = DataSource.GetInitialDataSource(appId: appId, parentLog:parentLog);
            var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: source);
            typeFilter.TypeName = Constants.DataPipelineStaticName;

	        var dict = new Dictionary<string, IDataSource>(StringComparer.OrdinalIgnoreCase);
	        foreach (var entQuery in typeFilter.List)
	        {
	            var delayedQuery = new DeferredPipelineQuery(zoneId, appId, entQuery, valuesCollectionProvider);
                dict.Add(entQuery.Title[0].ToString(), delayedQuery);
	        }
	        return dict;
	    }

        
	}
}

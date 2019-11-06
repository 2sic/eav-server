using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Query;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.ValueProviders;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Pipeline
{
	/// <summary>
	/// Helpers to work with Data Queries
	/// </summary>
	public class DataQuery
	{
	    /// <summary>
		/// Get an Entity Describing a Query
		/// </summary>
		/// <param name="entityId">EntityId</param>
		/// <param name="dataSource">DataSource to load Entity from</param>
		public static IEntity GetQueryEntity(int entityId, IDataSource dataSource)
		{
			try
			{
			    var queryEntity = dataSource.List.FindRepoId(entityId);
                if (queryEntity.Type.StaticName != Constants.QueryTypeName)
                    throw new ArgumentException("Entity is not an DataQuery Entity", nameof(entityId));
			    return queryEntity;
			}
			catch (Exception)
			{
				throw new ArgumentException($"Could not load Query-Entity with ID {entityId}.", nameof(entityId));
			}

		}

        /// <summary>
        /// Assembles a list of all queries / Querys configured for this app. 
        /// The queries internally are not assembled yet for performance reasons...
        /// ...but will be auto-assembled the moment they are accessed
        /// </summary>
        /// <returns></returns>
	    public static Dictionary<string, IDataSource> AllQueries(int zoneId, int appId, ITokenListFiller valuesCollectionProvider, ILog parentLog, bool showDrafts)
	    {
	        var dict = new Dictionary<string, IDataSource>(StringComparer.OrdinalIgnoreCase);
	        foreach (var entQuery in AllQueryItems(appId, parentLog))
	        {
	            var delayedQuery = new DeferredQuery(zoneId, appId, entQuery, valuesCollectionProvider, showDrafts);
                // make sure it doesn't break if two queries have the same name...
	            var name = entQuery.Title[0].ToString();
	            if (!dict.ContainsKey(name))
	                dict[name] = delayedQuery;
	        }
	        return dict;
	    }

	    public static IEnumerable<IEntity> AllQueryItems(int appId, ILog parentLog)
	    {
	        var source = DataSource.GetInitialDataSource(appId: appId, parentLog: parentLog);
	        var typeFilter = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: source);
	        typeFilter.TypeName = Constants.QueryTypeName;
	        var list = typeFilter.List;
	        return list;
	    }
	}
}

using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Queries
{
	/// <summary>
	/// Helpers to work with Data Queries
	/// </summary>
	[PrivateApi]
	internal static class QueryManager
	{
	    /// <summary>
		/// Get an Entity Describing a Query
		/// </summary>
		/// <param name="entityId">EntityId</param>
		/// <param name="dataSource">DataSource to load Entity from</param>
		internal static IEntity GetQueryEntity(int entityId, AppState dataSource)
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
        /// Assembles a list of all queries / Queries configured for this app. 
        /// The queries internally are not assembled yet for performance reasons...
        /// ...but will be auto-assembled the moment they are accessed
        /// </summary>
        /// <returns></returns>
	    internal static Dictionary<string, IQuery> AllQueries(/*int zoneId, int appId*/IAppIdentity app, ILookUpEngine valuesCollectionProvider, ILog parentLog, bool showDrafts)
	    {
	        var dict = new Dictionary<string, IQuery>(StringComparer.OrdinalIgnoreCase);
	        foreach (var entQuery in AllQueryItems(app, parentLog))
	        {
	            var delayedQuery = new Query(app.ZoneId, app.AppId, entQuery, valuesCollectionProvider, showDrafts, null, parentLog);
                // make sure it doesn't break if two queries have the same name...
	            var name = entQuery.Title[0].ToString();
	            if (!dict.ContainsKey(name))
	                dict[name] = delayedQuery;
	        }
	        return dict;
	    }

	    internal static IEnumerable<IEntity> AllQueryItems(IAppIdentity app, ILog parentLog)
	    {
            var dsFact = new DataSource(parentLog);
	        var source = dsFact.GetPublishing(app);
	        var typeFilter = dsFact.GetDataSource<EntityTypeFilter>(source);
	        typeFilter.TypeName = Constants.QueryTypeName;
	        var list = typeFilter.List;
	        return list;
	    }
	}
}

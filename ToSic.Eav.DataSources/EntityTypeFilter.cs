using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Return only entities of a specific type
	/// </summary>
	[PipelineDesigner]
	public class EntityTypeFilter : BaseDataSource
	{
		#region Configuration-properties
		private const string TypeNameKey = "TypeName";

		/// <summary>
		/// The name of the type to filter for. 
		/// </summary>
		public string TypeName
		{
			get { return Configuration[TypeNameKey]; }
			set { Configuration[TypeNameKey] = value; }
		}
		#endregion

		/// <summary>
		/// Constructs a new EntityTypeFilter
		/// </summary>
		public EntityTypeFilter()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetList));
			Configuration.Add(TypeNameKey, "[Settings:TypeName]");
        
            CacheRelevantConfigurations = new[] { TypeNameKey };
        }

	    private IEnumerable<IEntity> GetList()
	    {
	        EnsureConfigurationIsLoaded();

            // 2015-04-24 had to deactivate cache-method, because not available during testing
            //// Try to use real cache-id if it has a cache - probably faster and more precise
	        try
	        {
	            var cache = DataSource.GetCache(ZoneId, AppId);
	            var foundType = cache?.GetContentType(TypeName);
	            if (foundType != null) // maybe it doesn't find it!
	                return (from e in In[Constants.DefaultStreamName].LightList
	                    where e.Type == foundType
	                    select e);
	        }
	        // ReSharper disable once EmptyGeneralCatchClause
	        catch
	        {
	        }

            // This is the fallback, probably slower. In this case, it tries to match the name instead of the real type
            // Reason is that many dynamically created content-types won't be known to the cache, so they cannot be found the previous way
	        return (from e in In[Constants.DefaultStreamName].LightList
	            where e.Type.Name == TypeName
	            select e);
	    }

	}
}
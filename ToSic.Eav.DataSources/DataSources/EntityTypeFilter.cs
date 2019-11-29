using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Query;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Return only entities of a specific content-type
	/// </summary>
    [PublicApi]
	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.EntityTypeFilter, ToSic.Eav.DataSources",
        Type = DataSourceType.Filter, 
        DynamicOut = false,
        NiceName = "ContentTypeFilter",
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.EntityTypeFilter",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-ContentTypeFilter")]

    public class EntityTypeFilter : BaseDataSource
	{
        #region Configuration-properties
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.TypeF";

        private const string TypeNameKey = "TypeName";

		/// <summary>
		/// The name of the type to filter for. Either the normal name or the 'StaticName' which is usually a GUID.
		/// </summary>
		public string TypeName
		{
			get => Configuration[TypeNameKey];
		    set => Configuration[TypeNameKey] = value;
		}
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityTypeFilter
        /// </summary>
        [PrivateApi]
        public EntityTypeFilter()
		{
            Provide(GetList);
		    ConfigMask(TypeNameKey, "[Settings:TypeName]");
        }

        // special alternately named stream for use in the App data-source
        internal void AddNamedStream(string otherName) => Out.Add(otherName, new DataStream(this, otherName, GetList));

	    private IEnumerable<IEntity> GetList()
	    {
	        EnsureConfigurationIsLoaded();
	        Log.Add($"get list with type:{TypeName}");

	        try
	        {
	            var cache = DataSource.GetCache(ZoneId, AppId);
	            var foundType = cache?.GetContentType(TypeName);
	            if (foundType != null) // maybe it doesn't find it!
	                return (from e in In[Constants.DefaultStreamName].List
	                    where e.Type == foundType
	                    select e);
	        }
	        catch { /* ignore */ }

            // This is the fallback, probably slower. In this case, it tries to match the name instead of the real type
            // Reason is that many dynamically created content-types won't be known to the cache, so they cannot be found the previous way
	        return (from e in In[Constants.DefaultStreamName].List
	            where e.Type.Name == TypeName
	            select e);
	    }

	}
}
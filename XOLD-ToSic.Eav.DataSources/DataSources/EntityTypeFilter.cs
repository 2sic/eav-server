using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Return only entities of a specific content-type
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.EntityTypeFilter, ToSic.Eav.DataSources",
        Type = DataSourceType.Filter, 
        DynamicOut = false,
        NiceName = "ContentType-Filter",
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.EntityTypeFilter",
        HelpLink = "https://r.2sxc.org/DsTypeFilter")]

    public class EntityTypeFilter : DataSourceBase
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

		// 2020-03-02 2dm disabled this, not necessary any more
        //// special alternately named stream for use in the App data-source
        //internal void AddNamedStream(string otherName) => Out.Add(otherName, new DataStream(this, otherName, GetList));

	    private List<IEntity> GetList()
	    {
            Configuration.Parse();
            Log.Add($"get list with type:{TypeName}");

	        try
            {
                var appState = Apps.State.Get(this);
	            var foundType = appState?.GetContentType(TypeName);
	            if (foundType != null) // maybe it doesn't find it!
	                return (from e in In[Constants.DefaultStreamName].List
	                    where e.Type == foundType
	                    select e)
                        .ToList();
	        }
	        catch { /* ignore */ }

            // This is the fallback, probably slower. In this case, it tries to match the name instead of the real type
            // Reason is that many dynamically created content-types won't be known to the cache, so they cannot be found the previous way
	        return (from e in In[Constants.DefaultStreamName].List
	            where e.Type.Name == TypeName
	            select e)
                .ToList();
	    }

	}
}
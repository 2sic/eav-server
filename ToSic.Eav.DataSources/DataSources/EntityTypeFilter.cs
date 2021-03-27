using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Keep only entities of a specific content-type
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Type-Filter",
        UiHint = "Only keep items of the specified type",
        Icon = "alt_route",
        Type = DataSourceType.Filter, 
        GlobalName = "ToSic.Eav.DataSources.EntityTypeFilter, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { Constants.DefaultStreamName },
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

	    private ImmutableArray<IEntity> GetList()
	    {
            Configuration.Parse();
            Log.Add($"get list with type:{TypeName}");

	        try
            {
                var appState = Apps.State.Get(this);
	            var foundType = appState?.GetContentType(TypeName);
	            if (foundType != null) // maybe it doesn't find it!
	                return In[Constants.DefaultStreamName].List
                        .OfType(foundType)
                        .ToImmutableArray();
	        }
	        catch { /* ignore */ }

            // This is the fallback, probably slower. In this case, it tries to match the name instead of the real type
            // Reason is that many dynamically created content-types won't be known to the cache, so they cannot be found the previous way
	        return In[Constants.DefaultStreamName].List
                .OfType(TypeName)
                .ToImmutableArray();
	    }

	}
}
using System.Collections.Generic;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Filter entities to show Drafts or only Published Entities
	/// </summary>

	[VisualQuery(Type = DataSourceType.Security, Icon = "eye", DynamicOut = false, 
        EnableConfig = false,
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-PublishingFilter",
        ExpectsDataOfType = null)]

    public class PublishingFilter : BaseDataSource
	{
        #region Configuration-properties
	    public override string LogId => "DS.Publsh";

        private const string ShowDraftsKey = "ShowDrafts";

		/// <summary>
		/// Indicates whether to show drafts or only Published Entities
		/// </summary>
		public bool ShowDrafts
		{
			get => bool.Parse(Configuration[ShowDraftsKey]);
		    set => Configuration[ShowDraftsKey] = value.ToString();
		}
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new PublishingFilter
		/// </summary>
		public PublishingFilter()
		{
            Provide(() => DataStream().List);
            //Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));
		    ConfigMask(ShowDraftsKey, "[Settings:ShowDrafts||false]");

            //CacheRelevantConfigurations = new[] { ShowDraftsKey };

 	        //IEnumerable<IEntity> GetList() => DataStream().List;
       }


	    private IDataStream DataStream()
	    {
	        EnsureConfigurationIsLoaded();
	        Log.Add($"get incl. draft:{ShowDrafts}");
	        var outStreamName = ShowDrafts ? Constants.DraftsStreamName : Constants.PublishedStreamName;
	        return In[outStreamName];
	    }

	}
}
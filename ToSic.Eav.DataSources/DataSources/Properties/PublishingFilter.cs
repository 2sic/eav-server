using System.Collections.Immutable;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Filter entities to show Drafts or only Published Entities
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]

	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.PublishingFilter, ToSic.Eav.DataSources",
        Type = DataSourceType.Security, 
        NiceName = "Publishing Filter",
        Icon = "visibility", 
        In = new []{ Constants.PublishedStreamName, Constants.DefaultStreamName,  Constants.DraftsStreamName },
        DynamicOut = false, 
        HelpLink = "https://r.2sxc.org/DsPublishingFilter")]

    public class PublishingFilter : DataSourceBase
	{
        #region Configuration-properties
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.Publsh";

		/// <summary>
		/// Indicates whether to show drafts or only Published Entities. 
		/// </summary>
		public bool ShowDrafts
		{
			get => bool.Parse(Configuration[QueryConstants.ParamsShowDraftKey]);
		    set => Configuration[QueryConstants.ParamsShowDraftKey] = value.ToString();
		}
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new PublishingFilter
		/// </summary>
		[PrivateApi]
		public PublishingFilter()
		{
            Provide(PublishingFilterList);
		    ConfigMask(QueryConstants.ParamsShowDraftKey, "[Settings:ShowDrafts||false]");
       }


	    private IImmutableList<IEntity> PublishingFilterList()
	    {
            Configuration.Parse();
            Log.Add($"get incl. draft:{ShowDrafts}");
	        var outStreamName = ShowDrafts 
                ? Constants.DraftsStreamName 
                : Constants.PublishedStreamName;
	        return In[outStreamName].Immutable;
	    }

	}
}
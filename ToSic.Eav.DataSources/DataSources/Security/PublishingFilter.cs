﻿using System.Collections.Immutable;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Filter entities to show Drafts or only Published Entities
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]

	[VisualQuery(
        NiceName = "Publishing Filter",
        UiHint = "Keep data based on user roles (editor sees draft items)",
        Icon = Icons.Eye, 
        Type = DataSourceType.Security, 
        GlobalName = "ToSic.Eav.DataSources.PublishingFilter, ToSic.Eav.DataSources",
        In = new []{ Constants.PublishedStreamName + "*", Constants.DefaultStreamName + "*",  Constants.DraftsStreamName + "*" },
        DynamicOut = false, 
        HelpLink = "https://r.2sxc.org/DsPublishingFilter")]

    public class PublishingFilter : DataSource
	{
        #region Configuration-properties

		/// <summary>
		/// Indicates whether to show drafts or only Published Entities. 
		/// </summary>
		public bool ShowDrafts
		{
			get => Configuration.GetThis(QueryConstants.ShowDraftsDefault);
            set => Configuration.SetThis(value);
        }
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new PublishingFilter
		/// </summary>
		[PrivateApi]
		public PublishingFilter(Dependencies dependencies) : base(dependencies, $"{DataSourceConstants.LogPrefix}.Publsh")
        {
            Provide(PublishingFilterList);
		    ConfigMask(QueryConstants.ParamsShowDraftKeyAndToken);
       }


	    private IImmutableList<IEntity> PublishingFilterList()
	    {
            Configuration.Parse();
            Log.A($"get incl. draft:{ShowDrafts}");
	        var outStreamName = ShowDrafts 
                ? Constants.DraftsStreamName 
                : Constants.PublishedStreamName;
	        return In[outStreamName].List.ToImmutableList();
	    }

	}
}
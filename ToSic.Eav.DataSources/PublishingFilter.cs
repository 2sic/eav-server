﻿using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Filter entities to show Drafts or only Published Entities
	/// </summary>
	[PublicApi]

	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.PublishingFilter, ToSic.Eav.DataSources",
        Type = DataSourceType.Security, 
        Icon = "eye", 
        DynamicOut = false, 
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-PublishingFilter")]

    public class PublishingFilter : BaseDataSource
	{
        #region Configuration-properties
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.Publsh";

        private const string ShowDraftsKey = "ShowDrafts";

		/// <summary>
		/// Indicates whether to show drafts or only Published Entities. 
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
		[PrivateApi]
		public PublishingFilter()
		{
            Provide(GetList);
		    ConfigMask(ShowDraftsKey, "[Settings:ShowDrafts||false]");
       }


	    private IEnumerable<IEntity> GetList()
	    {
	        EnsureConfigurationIsLoaded();
	        Log.Add($"get incl. draft:{ShowDrafts}");
	        var outStreamName = ShowDrafts 
                ? Constants.DraftsStreamName 
                : Constants.PublishedStreamName;
	        return In[outStreamName].List;
	    }

	}
}
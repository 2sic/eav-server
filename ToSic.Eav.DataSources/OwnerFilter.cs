using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Attributes;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Filter entities to show Drafts or only Published Entities
	/// </summary>
	[PipelineDesigner]
	[DataSourceProperties(Type = DataSourceType.Security, DynamicOut = false,
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-OwnerFilter")]

    public class OwnerFilter : BaseDataSource
	{
        #region Configuration-properties
	    public override string LogId => "DS.OwnrF";

        private const string IdentityCode = "IdentityCode";

        /// <summary>
        /// Indicates whether to show drafts or only Published Entities
        /// </summary>
        public string Identity
		{
			get => Configuration[IdentityCode];
            set => Configuration[IdentityCode] = value;
        }
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new PublishingFilter
		/// </summary>
		public OwnerFilter()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));
			Configuration.Add(IdentityCode, "[Settings:" + IdentityCode + "]"); 

            CacheRelevantConfigurations = new[] { IdentityCode };
        }

        private IEnumerable<IEntity> GetList()
        {
            EnsureConfigurationIsLoaded();

            Log.Add($"get for identity:{Identity}");
            if (string.IsNullOrWhiteSpace(Identity)) return new List<IEntity>();

            return In["Default"].List.Where(e => e.Owner == Identity);
        }

	}
}
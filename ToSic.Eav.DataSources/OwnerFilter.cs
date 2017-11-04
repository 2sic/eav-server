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
	[DataSourceProperties(Type = DataSourceType.Security)]

    public class OwnerFilter : BaseDataSource
	{
        #region Configuration-properties
	    public override string LogId => "DS.OwnrF";

        private const string _identityCode = "IdentityCode";

        /// <summary>
        /// Indicates whether to show drafts or only Published Entities
        /// </summary>
        public string Identity
		{
			get => Configuration[_identityCode];
            set => Configuration[_identityCode] = value;
        }
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new PublishingFilter
		/// </summary>
		public OwnerFilter()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetList));
			Configuration.Add(_identityCode, "[Settings:" + _identityCode + "]"); 

            CacheRelevantConfigurations = new[] { _identityCode };
        }

        private IEnumerable<IEntity> GetList()
        {
            EnsureConfigurationIsLoaded();

            Log.Add($"get for identity:{Identity}");
            if (string.IsNullOrWhiteSpace(Identity)) return new List<IEntity>();

            return In["Default"].LightList.Where(e => e.Owner == Identity);
        }

	}
}
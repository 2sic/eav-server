using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Filter entities to show Drafts or only Published Entities
	/// </summary>
	[PipelineDesigner]
	public class OwnerFilter : BaseDataSource
	{
		#region Configuration-properties
		private const string _identityCode = "IdentityCode";

        /// <summary>
        /// Indicates whether to show drafts or only Published Entities
        /// </summary>
        public string Identity
		{
			get { return Configuration[_identityCode]; }
			set { Configuration[_identityCode] = value; }
		}
		#endregion

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

            if (string.IsNullOrWhiteSpace(Identity))
                return new List<IEntity>();

            return In["Default"].LightList.Where(e => e.Owner == Identity);
        }

	}
}
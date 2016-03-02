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
		private const string UserIdKey = "UserId";

	    // private const string DefaultUnlikelyUserId = "TW6O0YAjPgA8DYt9GjfB"; // use a very random sequence

        /// <summary>
        /// Indicates whether to show drafts or only Published Entities
        /// </summary>
        public string UserId
		{
			get { return Configuration[UserIdKey]; }
			set { Configuration[UserIdKey] = value; }
		}
		#endregion

		/// <summary>
		/// Constructs a new PublishingFilter
		/// </summary>
		public OwnerFilter()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetList));
			Configuration.Add(UserIdKey, "[Settings:UserId]"); 

            CacheRelevantConfigurations = new[] { UserIdKey };
        }

        //private IDictionary<int, IEntity> GetEntities()
        //{
        //    return DataStream().List;
        //}

        private IEnumerable<IEntity> GetList()
        {
            EnsureConfigurationIsLoaded();

            if (string.IsNullOrWhiteSpace(UserId))
                return new List<IEntity>();

            return In["Default"].LightList.Where(e => e.Owner == UserId);
        }

	}
}
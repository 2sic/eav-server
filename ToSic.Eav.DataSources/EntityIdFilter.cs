using System;
using System.Collections.Generic;
// using System.Data.EntityClient;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// A DataSource that filters Entities by Ids
	/// </summary>
	[PipelineDesigner]
	public class EntityIdFilter : BaseDataSource
	{
		#region Configuration-properties
		private const string EntityIdKey = "EntityIds";
		//private const string PassThroughOnEmptyEntityIdsKey = "PassThroughOnEmptyEntityIds";

		/// <summary>
		/// A string containing one or more entity-ids. like "27" or "27,40,3063,30306"
		/// </summary>
		public string EntityIds
		{
			get { return Configuration[EntityIdKey]; }
			set { Configuration[EntityIdKey] = value; }
		}

		#endregion

		/// <summary>
		/// Constructs a new EntityIdFilter
		/// </summary>
		public EntityIdFilter()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetEntities));
			Configuration.Add(EntityIdKey, "[Settings:EntityIds]");
			//Configuration.Add(PassThroughOnEmptyEntityIdsKey, "[Settings:PassThroughOnEmptyEntityIds||false]");

            CacheRelevantConfigurations = new[] { EntityIdKey };
		}

		private IDictionary<int, IEntity> GetEntities()
		{
            EnsureConfigurationIsLoaded();

		    var entityIds = _cleanedIds;

		    var originals = In[Constants.DefaultStreamName].List;

			return entityIds.Where(originals.ContainsKey).ToDictionary(id => id, id => originals[id]);
		}

	    private IEnumerable<int> _cleanedIds;

	    protected internal override void EnsureConfigurationIsLoaded()
        {
            base.EnsureConfigurationIsLoaded();

            #region clean up list of IDs to remove all white-space etc.
            try
            {
                var configEntityIds = Configuration["EntityIds"];
                if (string.IsNullOrWhiteSpace(configEntityIds))
                    _cleanedIds = new int[0];
                else
                {
                    var lstEntityIds = new List<int>();
                    foreach (
                        var strEntityId in
                            Configuration["EntityIds"].Split(',').Where(strEntityId => !string.IsNullOrWhiteSpace(strEntityId)))
                    {
                        int entityIdToAdd;
                        if (int.TryParse(strEntityId, out entityIdToAdd))
                            lstEntityIds.Add(entityIdToAdd);
                    }

                    _cleanedIds = lstEntityIds.Distinct();//.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to load EntityIds from Configuration.", ex);
            }
            #endregion

            EntityIds = string.Join(",", _cleanedIds.Select(i => i.ToString()).ToArray());
        }

	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that filters Entities by Ids
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.EntityIdFilter, ToSic.Eav.DataSources",
        Type = DataSourceType.Filter, 
        DynamicOut = false,
        NiceName = "ItemIdFilter",
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.EntityIdFilter",
        HelpLink = "https://r.2sxc.org/DsIdFilter")]

    public class EntityIdFilter : DataSourceBase
	{
        #region Configuration-properties
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.EntIdF";

        private const string EntityIdKey = "EntityIds";
		//private const string PassThroughOnEmptyEntityIdsKey = "PassThroughOnEmptyEntityIds";

		/// <summary>
		/// A string containing one or more entity-ids. like "27" or "27,40,3063,30306"
		/// </summary>
		public string EntityIds
        {
            get => Configuration[EntityIdKey];
            set
            {
                // kill any spaces in the string
                var cleaned = Regex.Replace(value ?? "", @"\s+", "");
                Configuration[EntityIdKey] = cleaned;
            }
        }

        #endregion

		/// <summary>
		/// Constructs a new EntityIdFilter
		/// </summary>
		[PrivateApi]
		public EntityIdFilter()
		{
            Provide(GetList);
		    ConfigMask(EntityIdKey, "[Settings:EntityIds]");
		}

		private List<IEntity> GetList()
		{
            CustomConfigurationParse();

            var entityIds = _cleanedIds;

		    var originals = In[Constants.DefaultStreamName].List;

			//var result = entityIds.Where(originals.ContainsKey).ToDictionary(id => id, id => originals[id]);
		    var result = entityIds.Select(originals.One).Where(e => e != null).ToList();

		    Log.Add(() => $"get ids:[{string.Join(",",_cleanedIds)}] found:{result.Count()}");
		    return result;
		}

	    private IEnumerable<int> _cleanedIds;

        /// <inheritdoc/>
        [PrivateApi]
        private void CustomConfigurationParse()
        {
            Configuration.Parse();

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
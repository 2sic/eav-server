using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
	/// A DataSource that filters Entities by Ids. Can handle multiple IDs if comma-separated.
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Item Id Filter",
        UiHint = "Find items based on one or more IDs",
        Icon = "fingerprint",
        Type = DataSourceType.Filter, 
        GlobalName = "ToSic.Eav.DataSources.EntityIdFilter, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { Constants.DefaultStreamName },
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.EntityIdFilter",
        HelpLink = "https://r.2sxc.org/DsIdFilter")]

    public class EntityIdFilter : DataSourceBase
	{
        #region Configuration-properties
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.EntIdF";

        private const string EntityIdKey = "EntityIds";

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

		private ImmutableArray<IEntity> GetList()
        {
            var wrapLog = Log.Call<ImmutableArray<IEntity>>();

            var entityIds = CustomConfigurationParse();

            if (!ExceptionStream.IsDefaultOrEmpty)
                return wrapLog("error", ExceptionStream);


            var originals = In[Constants.DefaultStreamName].List;

		    var result = entityIds.Select(eid => originals.One(eid)).Where(e => e != null).ToImmutableArray();

		    Log.Add(() => $"get ids:[{string.Join(",",entityIds)}] found:{result.Length}");
            return wrapLog("ok", result);
        }

        [PrivateApi]
        private int[] CustomConfigurationParse()
        {
            var wrapLog = Log.Call<int[]>();
            Configuration.Parse();

            #region clean up list of IDs to remove all white-space etc.
            try
            {
                var configEntityIds = EntityIds;
                // check if we have anything to work with
                if (string.IsNullOrWhiteSpace(configEntityIds))
                    return wrapLog("empty", new int[0]);
                
                var preCleanedIds = configEntityIds
                    .Split(',')
                    .Where(strEntityId => !string.IsNullOrWhiteSpace(strEntityId));
                var lstEntityIds = new List<int>();
                foreach (var strEntityId in preCleanedIds)
                    if (int.TryParse(strEntityId, out var entityIdToAdd))
                        lstEntityIds.Add(entityIdToAdd);
                return wrapLog(EntityIds, lstEntityIds.Distinct().ToArray());
            }
            catch (Exception ex)
            {
                SetException("Can't find IDs", "Unable to load EntityIds from Configuration. Unexpected Exception.", ex);
                return wrapLog("error", null);
            }
            #endregion
        }

	}
}
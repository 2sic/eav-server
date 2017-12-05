﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Apps.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that returns the attributes of a content-type
    /// </summary>
    [VisualQuery(Type = DataSourceType.Source,
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false,
        EnableConfig = false,
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Zones")]

    public sealed class Zones: BaseDataSource
	{
        #region Configuration-properties (no config)
	    public override string LogId => "DS.EavZns";

	    private const string ZoneContentTypeName = "EAV_Zones";

	    // 2dm: this is for a later feature...
	    // ReSharper disable once UnusedMember.Local
        private const string ZoneCtGuid = "11001010-251c-eafe-2792-000000000001";


        private enum ZoneType
	    {
	        Id,
	        Name,
            TennantId,
            TennantName,
	        IsCurrent,
            DefaultAppId,
	        AppCount
	    }

        
		#endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Attributes DS
        /// </summary>
		public Zones()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));

            CacheRelevantConfigurations = new string[0];
		}

	    private IEnumerable<IEntity> GetList()
	    {
            //EnsureConfigurationIsLoaded();

            // Get cache, which manages a list of zones
	        var cache = (BaseCache)DataSource.GetCache(ZoneId, AppId);

	        var env = Factory.Resolve<IEnvironment>();

	        var list = cache.ZoneApps.Values.OrderBy(z => z.ZoneId).Select(zone =>
	        {
	            var tennant = env.ZoneMapper.Tennant(zone.ZoneId);

	            // Assemble the entities
	            var znData = new Dictionary<string, object>
	            {
                    {ZoneType.Id.ToString(), zone.ZoneId},
                    {ZoneType.Name.ToString(), $"Zone {zone.ZoneId}" },
	                {ZoneType.TennantId.ToString(), tennant?.Id},
	                {ZoneType.TennantName.ToString(), tennant?.Name},
                    {ZoneType.DefaultAppId.ToString(), zone.DefaultAppId },
                    {ZoneType.IsCurrent.ToString(), zone.ZoneId == ZoneId },
                    {ZoneType.AppCount.ToString(), zone.Apps.Count }
	            };

	            return new Data.Entity(AppId, zone.ZoneId, ZoneContentTypeName, znData, ZoneType.Name.ToString());
	        });

            return list;
        }

	}
}
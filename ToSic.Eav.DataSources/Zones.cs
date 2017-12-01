using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that returns the attributes of a content-type
	/// </summary>
	//[VisualQuery(Type = DataSourceType.Source,
	//    Difficulty = DifficultyBeta.Advanced, 
 //       DynamicOut = false,
 //       EnableConfig = false,
	//    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Zones")]

    public sealed class Zones: BaseDataSource
	{
        #region Configuration-properties (no config)
	    public override string LogId => "DS.EavZns";

        // private const string ZonesKey = "ContentType";
	    // private const string DefContentType = "";
	    private const string ZoneContentTypeName = "EAV_Zones";

	    // 2dm: this is for a later feature...
        private const string ZoneCtGuid = "11001010-251c-eafe-2792-000000000001";


        private enum ZoneType
	    {
	        Id,
	        Name,
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

	        var list = cache.ZoneApps.Values.OrderBy(z => z.ZoneId).Select(zone =>
	        {
	            // Assemble the entities
	            var znData = new Dictionary<string, object>
	            {
                    {ZoneType.Id.ToString(), zone.ZoneId},
                    {ZoneType.Name.ToString(), $"Zone {zone.ZoneId}" },
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
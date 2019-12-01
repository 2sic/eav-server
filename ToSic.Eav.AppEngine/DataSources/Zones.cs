using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.DataSources.Types;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.Query;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.System
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that gets all zones in the system
    /// </summary>
    [VisualQuery(
        GlobalName = "ToSic.Eav.DataSources.System.Zones, ToSic.Eav.Apps",
        Type = DataSourceType.Source,
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false,
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Zones")]

    public sealed class Zones: DataSourceBase
	{
        #region Configuration-properties (no config)
	    public override string LogId => "DS.EavZns";

	    private const string ZoneContentTypeName = "EAV_Zones";

	    // 2dm: this is for a later feature...
	    // ReSharper disable once UnusedMember.Local
        private const string ZoneCtGuid = "11001010-251c-eafe-2792-000000000001";

        
		#endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Attributes DS
        /// </summary>
		public Zones()
		{
            Provide(GetList);
		}

	    private IEnumerable<IEntity> GetList()
	    {
            // Get cache, which manages a list of zones
	        var cache = (BaseCache)DataSource.GetCache(ZoneId, AppId);

	        var env = Factory.Resolve<IAppEnvironment>();

	        var list = cache.ZoneApps.Values.OrderBy(z => z.ZoneId).Select(zone =>
	        {
	            var tenant = env.ZoneMapper.Tenant(zone.ZoneId);

	            // Assemble the entities
	            var znData = new Dictionary<string, object>
	            {
                    {ZoneType.Id.ToString(), zone.ZoneId},
                    {ZoneType.Name.ToString(), $"Zone {zone.ZoneId}" },
	                {ZoneType.TenantId.ToString(), tenant?.Id},
	                {ZoneType.TenantName.ToString(), tenant?.Name},
                    {ZoneType.DefaultAppId.ToString(), zone.DefaultAppId },
                    {ZoneType.IsCurrent.ToString(), zone.ZoneId == ZoneId },
                    {ZoneType.AppCount.ToString(), zone.Apps.Count }
	            };

	            return AsEntity(znData, ZoneType.Name.ToString(), ZoneContentTypeName, zone.ZoneId);
	        });

            return list;
        }

	}
}
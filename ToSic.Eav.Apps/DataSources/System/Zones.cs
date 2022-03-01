using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Eav.Documentation;
using ToSic.Eav.Run;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that gets all zones in the system.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    [VisualQuery(
        NiceName = "Zones",
        UiHint = "Zones of an installation",
        Icon = "border_outer",
        Type = DataSourceType.System,
        GlobalName = "ToSic.Eav.DataSources.System.Zones, ToSic.Eav.Apps",
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false,
        PreviousNames = new []
            {
                "ToSic.Eav.DataSources.System.Zones, ToSic.Eav.Apps",
                // not sure if this was ever used...just added it for safety for now
                // can probably remove again, if we see that all system queries use the correct name
                "ToSic.Eav.DataSources.Zones, ToSic.Eav.Apps",
            },
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Zones")]
    // ReSharper disable once UnusedMember.Global
    public sealed class Zones: DataSourceBase
	{
        #region Configuration-properties (no config)
	    public override string LogId => "DS.EavZns";

	    private const string ZoneContentTypeName = "EAV_Zones";

		#endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Zones DS
        /// </summary>
        [PrivateApi]
		public Zones(IZoneMapper zoneMapper, IAppStates appStates)
        {
            _zoneMapper = zoneMapper;
            _appStates = appStates;
            Provide(GetList);
        }
        private readonly IZoneMapper _zoneMapper;
        private readonly IAppStates _appStates;


        private ImmutableArray<IEntity> GetList()
        {
            var wrapLog = Log.Call<ImmutableArray<IEntity>>();
            
            // Get cache, which manages a list of zones
            var zones = _appStates.Zones;
            var builder = DataBuilder;
            var list = zones.Values.OrderBy(z => z.ZoneId).Select(zone =>
	        {
	            var site = _zoneMapper.SiteOfZone(zone.ZoneId);

	            // Assemble the entities
	            var znData = new Dictionary<string, object>
	            {
                    {ZoneType.Id.ToString(), zone.ZoneId},
                    {ZoneType.Name.ToString(), $"Zone {zone.ZoneId}" },
	                {ZoneType.TenantId.ToString(), site?.Id},
	                {ZoneType.TenantName.ToString(), site?.Name},
                    {ZoneType.DefaultAppId.ToString(), zone.DefaultAppId },
                    {ZoneType.PrimaryAppId.ToString(), zone.PrimaryAppId },
                    {ZoneType.IsCurrent.ToString(), zone.ZoneId == ZoneId },
                    {ZoneType.AppCount.ToString(), zone.Apps.Count }
	            };

                return builder.Entity(znData,
                    appId: 0, 
                    id:zone.ZoneId, 
                    titleField: ZoneType.Name.ToString(), 
                    typeName: ZoneContentTypeName);
            });
            var results = list.ToImmutableArray();
            return wrapLog($"{results.Length}", results);
        }

	}
}
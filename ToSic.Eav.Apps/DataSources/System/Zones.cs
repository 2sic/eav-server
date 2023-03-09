using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Lib.Logging;
using ToSic.Eav.Run;
using ToSic.Lib.Documentation;
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
        Icon = Icons.BorderOuter,
        Type = DataSourceType.System,
        GlobalName = "ToSic.Eav.DataSources.System.Zones, ToSic.Eav.Apps",
        Audience = Audience.Advanced,
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
    public sealed class Zones: DataSource
	{
        private readonly IDataFactory _dataFactory;

        #region Configuration-properties (no config)

	    private const string ZoneContentTypeName = "EAV_Zones";

		#endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Zones DS
        /// </summary>
        [PrivateApi]
		public Zones(MyServices services, IZoneMapper zoneMapper, IAppStates appStates, IDataFactory dataFactory): base(services, $"{DataSourceConstants.LogPrefix}.Zones")
        {
            ConnectServices(
                _zoneMapper = zoneMapper,
                _appStates = appStates,
                _dataFactory = dataFactory.Configure(appId: 0, typeName: ZoneContentTypeName, titleField: ZoneType.Name.ToString())
            );
            Provide(GetList);
        }
        private readonly IZoneMapper _zoneMapper;
        private readonly IAppStates _appStates;


        private IImmutableList<IEntity> GetList() => Log.Func(l =>
        {
            // Get cache, which manages a list of zones
            var zones = _appStates.Zones;
            var list = zones.Values.OrderBy(z => z.ZoneId).Select(zone =>
            {
                var site = _zoneMapper.SiteOfZone(zone.ZoneId);

                // Assemble the entities
                var znData = new Dictionary<string, object>
                {
                    {ZoneType.Id.ToString(), zone.ZoneId},
                    {ZoneType.Name.ToString(), $"Zone {zone.ZoneId}"},
                    {ZoneType.TenantId.ToString(), site?.Id},
                    {ZoneType.TenantName.ToString(), site?.Name},
                    {ZoneType.DefaultAppId.ToString(), zone.DefaultAppId},
                    {ZoneType.PrimaryAppId.ToString(), zone.PrimaryAppId},
                    {ZoneType.IsCurrent.ToString(), zone.ZoneId == ZoneId},
                    {ZoneType.AppCount.ToString(), zone.Apps.Count}
                };

                return _dataFactory.Create(znData, id: zone.ZoneId);
            });
            var results = list.ToImmutableList();
            return (results, $"{results.Count}");
        });

    }
}
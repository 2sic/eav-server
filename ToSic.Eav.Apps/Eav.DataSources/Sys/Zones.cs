using System.Collections.Immutable;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Eav.Integration;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys;

/// <inheritdoc />
/// <summary>
/// A DataSource that gets all zones in the system.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[VisualQuery(
    NiceName = "Zones",
    UiHint = "Zones of an installation",
    Icon = DataSourceIcons.BorderOuter,
    Type = DataSourceType.System,
    NameId = "ToSic.Eav.DataSources.System.Zones, ToSic.Eav.Apps",
    Audience = Audience.Advanced,
    DynamicOut = false,
    NameIds = new []
    {
        "ToSic.Eav.DataSources.System.Zones, ToSic.Eav.Apps",
        // not sure if this was ever used...just added it for safety for now
        // can probably remove again, if we see that all system queries use the correct name
        "ToSic.Eav.DataSources.Zones, ToSic.Eav.Apps",
    },
    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Zones")]
// ReSharper disable once UnusedMember.Global
public sealed class Zones: Eav.DataSource.DataSourceBase
{
    private readonly IDataFactory _dataFactory;

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
            _dataFactory = dataFactory.New(options: new(appId: 0, typeName: "Zone", titleField: ZoneType.Name.ToString()))
        );
        ProvideOut(GetList);
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
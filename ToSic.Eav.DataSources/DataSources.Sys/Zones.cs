using ToSic.Eav.Apps;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.DataSources.Sys.Types;
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
    NameIds =
    [
        "ToSic.Eav.DataSources.System.Zones, ToSic.Eav.Apps",
        // not sure if this was ever used...just added it for safety for now
        // can probably remove again, if we see that all system queries use the correct name
        "ToSic.Eav.DataSources.Zones, ToSic.Eav.Apps"
    ],
    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Zones")]
// ReSharper disable once UnusedMember.Global
public sealed class Zones: CustomDataSourceAdvanced
{
    /// <inheritdoc />
    /// <summary>
    /// Constructs a new Zones DS
    /// </summary>
    [PrivateApi]
    public Zones(MyServices services, IZoneMapper zoneMapper, IAppsCatalog appsCatalog)
        : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Zones", connect: [zoneMapper, appsCatalog])
    {
        _zoneMapper = zoneMapper;
        _appsCatalog = appsCatalog;
        ProvideOut(GetList);
    }
    private readonly IZoneMapper _zoneMapper;
    private readonly IAppsCatalog _appsCatalog;


    private IImmutableList<IEntity> GetList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        var dataFactory = DataFactory.SpawnNew(options: new()
        {
            AppId = 0,
            TitleField = ZoneType.Name.ToString(),
            TypeName = "Zone",
        });
        
        // Get cache, which manages a list of zones
        var zones = _appsCatalog.Zones;
        var results = zones.Values
            .OrderBy(z => z.ZoneId)
            .Select(zone =>
            {
                var site = _zoneMapper.SiteOfZone(zone.ZoneId);

                // Assemble the entities
                var znData = new Dictionary<string, object>
                {
                    { ZoneType.Id.ToString(), zone.ZoneId },
                    { ZoneType.Name.ToString(), $"Zone {zone.ZoneId}" },
                    { ZoneType.TenantId.ToString(), site?.Id },
                    { ZoneType.TenantName.ToString(), site?.Name },
                    { ZoneType.DefaultAppId.ToString(), zone.DefaultAppId },
                    { ZoneType.PrimaryAppId.ToString(), zone.PrimaryAppId },
                    { ZoneType.IsCurrent.ToString(), zone.ZoneId == ZoneId },
                    { ZoneType.AppCount.ToString(), zone.Apps.Count }
                };

                return dataFactory.Create(znData, id: zone.ZoneId);
            })
            .ToImmutableList();
        return l.Return(results, $"{results.Count}");
    }

}
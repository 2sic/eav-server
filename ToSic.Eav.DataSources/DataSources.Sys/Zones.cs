using ToSic.Eav.Apps;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.DataSource.Sys;


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
    NameId = "6edeedb4-db82-4afc-8e17-c29cd8b81770",
    Audience = Audience.Advanced,
    NameIds =
    [
        "ToSic.Eav.DataSources.System.Zones, ToSic.Eav.Apps",
        // not sure if this was ever used...just added it for safety for now
        // can probably remove again, if we see that all system queries use the correct name
        "ToSic.Eav.DataSources.Zones, ToSic.Eav.Apps"
    ],
    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Zones")]
// ReSharper disable once UnusedMember.Global
public sealed class Zones: CustomDataSource
{
    /// <inheritdoc />
    /// <summary>
    /// Constructs a new Zones DS
    /// </summary>
    [PrivateApi]
    public Zones(Dependencies services, IZoneMapper zoneMapper, IAppsCatalog appsCatalog)
        : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Zones", connect: [zoneMapper, appsCatalog])
    {
        ProvideOutRaw(() => GetList(zoneMapper, appsCatalog));
    }


    private IImmutableList<ZoneRaw> GetList(IZoneMapper zoneMapper, IAppsCatalog appsCatalog)
    {
        var l = Log.Fn<IImmutableList<ZoneRaw>>();
        
        // Get cache, which manages a list of zones
        var zones = appsCatalog.Zones;
        var results = zones.Values
            .OrderBy(z => z.ZoneId)
            .Select(zone =>
            {
                var site = zoneMapper.SiteOfZone(zone.ZoneId);

                return new ZoneRaw(
                    Id: zone.ZoneId,
                    Name: $"Zone {zone.ZoneId}",
                    TenantId: site?.Id,
                    TenantName: site?.Name,
                    DefaultAppId: zone.DefaultAppId,
                    PrimaryAppId: zone.PrimaryAppId,
                    IsCurrent: zone.ZoneId == ZoneId,
                    AppCount: zone.Apps.Count
                );
            })
            .ToImmutableOpt();
        return l.Return(results, $"{results.Count}");
    }

}
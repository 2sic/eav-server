
using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;

namespace ToSic.Eav.WebApi.Sys.Languages;

[PrivateApi]
[VisualQuery(
    NiceName = "Zone Languages",
    NameId = "9981db56-6d4e-4b29-914c-4a223c063eb4",
    NameIds = ["System.ZoneLanguages"], // Internal name for the system, used in the Admin-UI.
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Confidential,
    UiHint = "Languages of the current site"
)]
// ReSharper disable once UnusedMember.Global
public class ZoneLanguages(CustomDataSource.Dependencies services, IZoneMapper zoneMapper, ISite site)
    : CustomDataSource(services, logName: "Sxc.ZoneLangs", connect: [zoneMapper, site])
{
    /// <summary>
    /// Retrieve zone cultures with activation state for the current site.
    /// </summary>
    protected override IEnumerable<IRawData> GetDefault()
        => zoneMapper.CulturesWithState(site).Cast<IRawEntityAutoConvert>();
}

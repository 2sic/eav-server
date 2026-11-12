using ToSic.Eav.DataSource.VisualQuery;

namespace ToSic.Eav.Apps.ImportExport.Sys;

[PrivateApi]
[VisualQuery(
    NiceName = "Apps Pending Initialization",
    NameId = "746f371d-6fd4-4834-a559-cb4a1ae2ec9e",
    NameIds = ["System.AppsPendingInitialization"],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.System,
    UiHint = "App packages waiting to be initialized")]
// ReSharper disable once UnusedMember.Global
public class AppsPendingInitialization(CustomDataSource.Dependencies services, LazySvc<PendingAppsGet> importApp)
    : CustomDataSource(services, "Sxc.PendingApps", connect: [importApp])
{
    /// <summary>
    /// The configuration for which zone to look in.
    /// Cannot be named `ZoneId` as that property already exists.
    /// </summary>
    [Configuration(Field = "ZoneId")]
    public int OfZoneId => Configuration.GetThis(ZoneId);

    protected override IEnumerable<IRawData> GetDefault()
        => importApp.Value.GetPendingApps(OfZoneId);

}

using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.ImportExport;

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
public class AppsPendingInitialization : CustomDataSource
{
    [Configuration(Field = "ZoneId")]
    public int OfZoneId => Configuration.GetThis(ZoneId);

    public AppsPendingInitialization(Dependencies services, LazySvc<PendingAppsGet> pendingAppsGet)
        : base(services, "Sxc.PendingApps", connect: [pendingAppsGet])
        => ProvideOutRaw(() => Get(pendingAppsGet), options: Options);

    private IEnumerable<PendingAppDto> Get(LazySvc<PendingAppsGet> pendingAppsGet)
        => pendingAppsGet.Value.GetPendingApps(OfZoneId);

    private static DataFactoryOptions Options() => new() { TypeName = "PendingApp", AllowUnknownValueTypes = true };
}

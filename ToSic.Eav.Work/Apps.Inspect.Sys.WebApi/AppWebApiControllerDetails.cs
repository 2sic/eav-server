using ToSic.Eav.Apps.Inspect.Sys.WebApi.Models;
using ToSic.Eav.DataSource.VisualQuery;

namespace ToSic.Eav.Apps.Inspect.Sys.WebApi;

[PrivateApi]
[VisualQuery(
    NiceName = "App WebApi Controller Details",
    NameId = "70179265-0a90-4605-953a-91d237bed938",
    NameIds = ["System.AppWebApiControllerDetails"],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Internal,
    UiHint = "Security details of a single App WebApi controller"
)]
public class AppWebApiControllerDetails(
    CustomDataSource.Dependencies services,
    AppWebApiControllerAnalyzer analyzer,
    IAppWebApiControllerAssemblyLoader assemblyLoader)
    : CustomDataSource(services, logName: "Eav.ApiCtlDet", connect: [analyzer, assemblyLoader])
{
    [Configuration(Fallback = "")]
    public string Path => Configuration.GetThis(fallback: "");

    protected override IEnumerable<IRawData> GetDefault()
    {
        var l = Log.Fn<IEnumerable<AppWebApiControllerRaw>>();

        if (string.IsNullOrWhiteSpace(Path))
            return l.Return([], "missing path");

        var assembly = assemblyLoader.GetAssembly(Path);
        var (controller, _) = analyzer.Analyze(Path, assembly);

        return l.Return([controller], "ok");
    }
}

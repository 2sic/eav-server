using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.WebApi.Sys.ApiExplorer;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Admin;

[PrivateApi]
[VisualQuery(
    NiceName = "App WebApi Controller Endpoints",
    NameId = "06efd171-7d8f-4752-8ced-e444c8247c70",
    NameIds = ["System.AppWebApiControllerEndpoints"],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Internal,
    UiHint = "Endpoints of a single App WebApi controller"
)]
public class AppWebApiControllerEndpoints(
    CustomDataSource.Dependencies services,
    AppWebApiControllerAnalyzer analyzer,
    IAppWebApiControllerAssemblyLoader assemblyLoader)
    : CustomDataSource(services, logName: "Eav.ApiCtlEp", connect: [analyzer, assemblyLoader])
{
    [Configuration(Fallback = "")]
    public string Path => Configuration.GetThis(fallback: "");

    protected override IEnumerable<IRawData> GetDefault()
    {
        var l = Log.Fn<IEnumerable<AppWebApiEndpointRaw>>();

        if (string.IsNullOrWhiteSpace(Path))
            return l.Return([], "missing path");

        var assembly = assemblyLoader.GetAssembly(Path);
        var (_, endpoints) = analyzer.Analyze(Path, assembly);

        return l.Return(endpoints, $"{endpoints.Count}");
    }

}

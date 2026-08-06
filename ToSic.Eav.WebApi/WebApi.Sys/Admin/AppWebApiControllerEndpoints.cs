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
public class AppWebApiControllerEndpoints : CustomDataSource
{
    private readonly AppWebApiControllerAnalyzer _analyzer;
    private readonly IAppWebApiControllerAssemblyLoader _assemblyLoader;

    [Configuration(Fallback = "")]
    public string Path => Configuration.GetThis(fallback: "");

    public AppWebApiControllerEndpoints(
        Dependencies services,
        AppWebApiControllerAnalyzer analyzer,
        IAppWebApiControllerAssemblyLoader assemblyLoader)
        : base(services, logName: "Eav.ApiCtlEp", connect: [analyzer, assemblyLoader])
    {
        _analyzer = analyzer;
        _assemblyLoader = assemblyLoader;

        ProvideOutRaw(GetEndpoints, options: () => new()
        {
            AllowUnknownValueTypes = true,
        });
    }

    private IEnumerable<AppWebApiEndpointRaw> GetEndpoints()
    {
        var l = Log.Fn<IEnumerable<AppWebApiEndpointRaw>>();

        if (string.IsNullOrWhiteSpace(Path))
            return l.Return([], "missing path");

        var assembly = _assemblyLoader.GetAssembly(Path);
        var (_, endpoints) = _analyzer.Analyze(Path, assembly);

        return l.Return(endpoints, $"{endpoints.Count}");
    }

}

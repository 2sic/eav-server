using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.WebApi.Sys.ApiExplorer;

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
            TitleField = nameof(ApiActionDto.name),
            TypeName = "AppWebApiControllerEndpoint",
            AllowUnknownValueTypes = true,
        });
    }

    private IEnumerable<AppWebApiEndpointModel> GetEndpoints()
    {
        var l = Log.Fn<IEnumerable<AppWebApiEndpointModel>>();

        if (string.IsNullOrWhiteSpace(Path))
            return l.Return([], "missing path");

        var assembly = _assemblyLoader.GetAssembly(Path);
        var dto = _analyzer.Analyze(Path, assembly);

        var entities = dto.actions.Select(action => new AppWebApiEndpointModel(action)).ToList();

        return l.Return(entities, $"{entities.Count}");
    }

}
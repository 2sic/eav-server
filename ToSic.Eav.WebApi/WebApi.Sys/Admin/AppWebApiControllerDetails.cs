using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.WebApi.Sys.ApiExplorer;

namespace ToSic.Eav.WebApi.Sys.Admin;

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
public class AppWebApiControllerDetails : CustomDataSource
{
    private readonly AppWebApiControllerAnalyzer _analyzer;
    private readonly IAppWebApiControllerAssemblyLoader _assemblyLoader;

    [Configuration(Fallback = "")]
    public string Path => Configuration.GetThis(fallback: "");

    public AppWebApiControllerDetails(
        Dependencies services,
        AppWebApiControllerAnalyzer analyzer,
        IAppWebApiControllerAssemblyLoader assemblyLoader)
        : base(services, logName: "Eav.ApiCtlDet", connect: [analyzer, assemblyLoader])
    {
        _analyzer = analyzer;
        _assemblyLoader = assemblyLoader;

        ProvideOutRaw(GetDetails, options: () => new()
        {
            TitleField = nameof(ApiControllerDto.controller),
            TypeName = "AppWebApiControllerDetails",
            AllowUnknownValueTypes = true,
        });
    }

    private IEnumerable<AppWebApiControllerModel> GetDetails()
    {
        var l = Log.Fn<IEnumerable<AppWebApiControllerModel>>();

        if (string.IsNullOrWhiteSpace(Path))
            return l.Return([], "missing path");

        var assembly = _assemblyLoader.GetAssembly(Path);
        var dto = _analyzer.Analyze(Path, assembly);
        var entity = new AppWebApiControllerModel(dto);

        return l.Return([entity], "ok");
    }
}
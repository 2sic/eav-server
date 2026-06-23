using ToSic.Eav.Data.Raw.Sys;
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

    private IEnumerable<IRawEntity> GetEndpoints()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();

        if (string.IsNullOrWhiteSpace(Path))
            return l.Return([], "missing path");

        var assembly = _assemblyLoader.GetAssembly(Path);
        var dto = _analyzer.Analyze(Path, assembly);

        var entities = dto.actions.Select(action =>
        {
            var mergedSecurity = action.mergedSecurity;

            return new RawEntity(new()
            {
                { nameof(ApiActionDto.name), action.name },
                { nameof(ApiActionDto.returns), action.returns },
                { nameof(ApiActionDto.verbs), string.Join(", ", action.verbs) },

                // Hacky sub-object
                { nameof(ApiActionDto.parameters), action.parameters },

                // Final merged security flat on endpoint item
                // TODO: @2rb deduplicate
                { nameof(ApiSecurityDto.ignoreSecurity), mergedSecurity.ignoreSecurity },
                { nameof(ApiSecurityDto.allowAnonymous), mergedSecurity.allowAnonymous },
                { nameof(ApiSecurityDto.requireVerificationToken), mergedSecurity.requireVerificationToken },
                { nameof(ApiSecurityDto.requireContext), mergedSecurity.requireContext },
                { nameof(ApiSecurityDto.view), mergedSecurity.view },
                { nameof(ApiSecurityDto.edit), mergedSecurity.edit },
                { nameof(ApiSecurityDto.admin), mergedSecurity.admin },
                { nameof(ApiSecurityDto.superUser), mergedSecurity.superUser },

                // Explicit endpoint security object
                { nameof(ApiActionDto.security), SecurityValues(action.security) },
            });
        }).ToList();

        return l.Return(entities, $"{entities.Count}");
    }

    // TODO: @2rb deduplicate - use this as foundation, make internal
    private static Dictionary<string, object> SecurityValues(ApiSecurityDto security) => new()
    {
        { nameof(ApiSecurityDto.ignoreSecurity), security.ignoreSecurity },
        { nameof(ApiSecurityDto.allowAnonymous), security.allowAnonymous },
        { nameof(ApiSecurityDto.requireVerificationToken), security.requireVerificationToken },
        { nameof(ApiSecurityDto.requireContext), security.requireContext },
        { nameof(ApiSecurityDto.view), security.view },
        { nameof(ApiSecurityDto.edit), security.edit },
        { nameof(ApiSecurityDto.admin), security.admin },
        { nameof(ApiSecurityDto.superUser), security.superUser },
    };
}
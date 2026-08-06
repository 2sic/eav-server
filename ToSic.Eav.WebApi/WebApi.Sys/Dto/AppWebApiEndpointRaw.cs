using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "AppWebApiControllerEndpoint",
    Guid = "06efd171-7d8f-4752-8ced-e444c8247c70",
    Description = "App WebApi controller endpoint",
    Scope = "System"
)]
public class AppWebApiEndpointRaw : IRawEntityAutoConvert
{
    [ContentTypeField(IsTitle = true)]
    public required string name { get; init; }

    public required string returns { get; init; }
    public required string verbs { get; init; }
    public required IEnumerable<Parameter> parameters { get; init; }
    public required Dictionary<string, object?> security { get; init; }

    public bool IgnoreSecurity { get; init; }
    public bool AllowAnonymous { get; init; }
    public bool RequireVerificationToken { get; init; }
    public bool RequireContext { get; init; }
    public bool View { get; init; }
    public bool Edit { get; init; }
    public bool Admin { get; init; }
    public bool SuperUser { get; init; }

    public class Parameter
    {
        public required string name { get; init; }
        public required string type { get; init; }
        public required object? defaultValue { get; init; }
        public required bool isOptional { get; init; }
        public required bool isBody { get; init; }
    }
}

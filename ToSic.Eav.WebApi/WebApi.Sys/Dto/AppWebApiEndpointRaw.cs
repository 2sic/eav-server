using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "AppWebApiControllerEndpoint",
    Guid = "06efd171-7d8f-4752-8ced-e444c8247c70",
    Description = "App WebApi controller endpoint",
    Scope = "System"
)]
public record AppWebApiEndpointRaw : IRawEntityAutoConvert
{
    [ContentTypeField(IsTitle = true)]
    public required string name { get; init; }

    public required string returns { get; init; }
    public required string verbs { get; init; }
    public required IEnumerable<Parameter> parameters { get; init; }
    public required IDictionary<string, object?> security { get; init; }

    public required bool ignoreSecurity { get; init; }
    public required bool allowAnonymous { get; init; }
    public required bool requireVerificationToken { get; init; }
    public required bool requireContext { get; init; }
    public required bool view { get; init; }
    public required bool edit { get; init; }
    public required bool admin { get; init; }
    public required bool superUser { get; init; }

    public record Parameter
    {
        public required string name { get; init; }
        public required string type { get; init; }
        public required object? defaultValue { get; init; }
        public required bool isOptional { get; init; }
        public required bool isBody { get; init; }
    }
}

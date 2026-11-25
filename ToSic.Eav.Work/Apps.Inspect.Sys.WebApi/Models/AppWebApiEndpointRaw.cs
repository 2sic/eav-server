using ToSic.Eav.Data.ContentTypes;

namespace ToSic.Eav.Apps.Inspect.Sys.WebApi.Models;

[ContentType(
    Name = "AppWebApiControllerEndpoint",
    Guid = "2c9d144a-5fde-4b62-8f31-3531bb789216",
    Description = "App WebApi controller endpoint",
    Scope = "System"
)]
public record AppWebApiEndpointRaw : IRawEntityAutoConvert
{
    [ContentTypeTitle]
    public required string name { get; init; }

    public required string returns { get; init; }
    public required string verbs { get; init; }
    [ContentTypeField(Type = ValueTypes.Object)]
    public required IEnumerable<Parameter> parameters { get; init; }
    [ContentTypeField(Type = ValueTypes.Object)]
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

using ToSic.Eav.Data.ContentTypes;

namespace ToSic.Eav.Apps.Inspect.Sys.WebApi.Models;

[ContentType(
    Name = "AppWebApiControllerDetails",
    Guid = "1fb5f818-188c-48f5-b61c-ebc0ff48ca81",
    Description = "App WebApi controller details",
    Scope = "System"
)]
public record AppWebApiControllerRaw : IRawEntityAutoConvert
{
    [ContentTypeTitle]
    public required string controller { get; init; }

    public required bool ignoreSecurity { get; init; }
    public required bool allowAnonymous { get; init; }
    public required bool requireVerificationToken { get; init; }
    public required bool requireContext { get; init; }
    public required bool view { get; init; }
    public required bool edit { get; init; }
    public required bool admin { get; init; }
    public required bool superUser { get; init; }
}

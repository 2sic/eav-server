using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "AppWebApiControllerDetails",
    Guid = "70179265-0a90-4605-953a-91d237bed938",
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

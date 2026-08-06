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
    [ContentTypeField(IsTitle = true)]
    public required string controller { get; init; }

    public required bool IgnoreSecurity { get; init; }
    public required bool AllowAnonymous { get; init; }
    public required bool RequireVerificationToken { get; init; }
    public required bool RequireContext { get; init; }
    public required bool View { get; init; }
    public required bool Edit { get; init; }
    public required bool Admin { get; init; }
    public required bool SuperUser { get; init; }
}

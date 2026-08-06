using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "AppWebApiControllerDetails",
    Guid = "70179265-0a90-4605-953a-91d237bed938",
    Description = "App WebApi controller details",
    Scope = "System"
)]
public class AppWebApiControllerRaw : IRawEntityAutoConvert
{
    [ContentTypeField(IsTitle = true)]
    public required string controller { get; init; }

    public bool IgnoreSecurity { get; init; }
    public bool AllowAnonymous { get; init; }
    public bool RequireVerificationToken { get; init; }
    public bool RequireContext { get; init; }
    public bool View { get; init; }
    public bool Edit { get; init; }
    public bool Admin { get; init; }
    public bool SuperUser { get; init; }
}

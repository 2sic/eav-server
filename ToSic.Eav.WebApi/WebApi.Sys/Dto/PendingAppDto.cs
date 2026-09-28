using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "PendingApp",
    Guid = "b84a5687-f70f-4ebd-a3ef-a8fccd026bef",
    Description = "App package waiting to be initialized",
    Scope = "System"
)]
public record PendingAppDto : IRawEntityAutoConvert
{
    // folder as it's stored on the server
    public required string ServerFolder { get; init; }
    // taken from the app.xml
    [ContentTypeTitle]
    public required string Name { get; init; }
    // taken from the app.xml
    public required string Description { get; init; }
    // taken from the app.xml
    public required string Version { get; init; }
    // taken from the app.xml
    public required string Folder { get; init; }
}

namespace ToSic.Eav.WebApi.Sys.Dto;

public class PendingAppDto
{
    // folder as it's stored on the server
    public required string ServerFolder { get; init; }
    // taken from the app.xml
    public required string Name { get; init; }
    // taken from the app.xml
    public required string Description { get; init; }
    // taken from the app.xml
    public required string Version { get; init; }
    // taken from the app.xml
    public required string Folder { get; init; }
}
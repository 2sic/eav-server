namespace ToSic.Eav.Apps.Inspect.Sys.Models;

public class AppMetadataDto
{
    public required int Id { get; init; }

    public required bool IsEnabled { get; init; }

    public required string Title { get; init; }
}
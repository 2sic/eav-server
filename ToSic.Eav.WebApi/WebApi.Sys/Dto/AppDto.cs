namespace ToSic.Eav.WebApi.Sys.Dto;

public class AppDto
{
    public required int Id { get; init; }
    public required bool IsApp { get; init; }
    public required string Guid { get; init; }
    public required string Name { get; init; }
    public required string Folder { get; init; }
    public required string AppRoot { get; init; }
    public required bool IsHidden { get; init; }
    public required int? ConfigurationId { get; init; }
    public required int Items { get; init; }
    public required string? Thumbnail { get; init; }
    public required string Version { get; init; }

    /// <summary>
    /// Determines if the App is global / should only use templates/resources in the global storage
    /// </summary>
    /// <remarks>New in 13.0</remarks>
    public required bool IsGlobal { get; init; }

    /// <summary>
    /// Determines if this app was inherited from another App
    /// </summary>
    public required bool IsInherited { get; init; }

    [JsonPropertyName("lightSpeed")]
    public required AppMetadataDto? Lightspeed { get; init; }

    public required bool HasCodeWarnings { get; init; }
}
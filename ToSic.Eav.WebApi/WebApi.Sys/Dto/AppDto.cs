namespace ToSic.Eav.WebApi.Dto;

public class AppDto
{
    public int Id { get; init; }
    public bool IsApp { get; init; }
    public string Guid { get; init; }
    public string Name { get; init; }
    public string Folder { get; init; }
    public string AppRoot { get; init; }
    public bool IsHidden { get; init; }
    public int? ConfigurationId { get; init; }
    public int Items { get; init; }
    public string Thumbnail { get; init; }
    public string Version { get; init; }

    /// <summary>
    /// Determines if the App is global / should only use templates/resources in the global storage
    /// </summary>
    /// <remarks>New in 13.0</remarks>
    public bool IsGlobal { get; init; }

    /// <summary>
    /// Determines if this app was inherited from another App
    /// </summary>
    public bool IsInherited { get; init; }

    [JsonPropertyName("lightSpeed")]
    public AppMetadataDto Lightspeed { get; init; }

    public bool HasCodeWarnings { get; init; }
}
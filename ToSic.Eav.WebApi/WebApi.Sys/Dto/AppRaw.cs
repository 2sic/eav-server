using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

// 1. Rename Raw
// 2. Define Content Type
// 3. Auto generate properties with IRawEntityAutoConvert
// 4. Specify which one is the title
//
// Change use
// - Simplify access/creation in DataSource
// - Make sure we don't need options (unless really important, because of special sub-objects)
//
// Then
// - verify it works
// - especially Guid & Id
//
// Cleanup
// - Delete anything not important (like the temporary objects)

[ContentType(
    Name = "App",
    Guid = "53b3fe9b-d689-4b1f-bed1-503cbc898ffc",
    Description = "App information",
    Scope = "System"
)]
public class AppRaw: IRawEntityAutoConvert
{
    public required int Id { get; init; }
    public required bool IsApp { get; init; }
    public required string Guid { get; init; }

    [ContentTypeTitle]
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
using ToSic.Eav.WebApi.Sys.Security;
using static System.Text.Json.Serialization.JsonIgnoreCondition;

namespace ToSic.Eav.WebApi.Sys.Dto;

public record ContextAppDto: WebResourceDto
{
    public required string Name { get; init; }
    public string? Identifier { get; init; }

    public string? SettingsScope { get; init; }

    public string? GettingStartedUrl { get; init; }
    public required string Folder { get; init; }
    public HasPermissionsDto? Permissions { get; init; }
        
    /// <summary>
    /// App API Root. Not 100% correct because it doesn't contain the App identity or anything
    /// But this tells the UI where to go work with things that need the App-API
    /// </summary>
    public string? Api { get; init; }

    /// <summary>
    /// Determines if this App is global, meaning it shouldn't have files in the site etc. just global
    /// </summary>
    /// <remarks>New in 13.0</remarks>
    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? IsShared { get; init; }

    /// <summary>
    /// Determines if this app was inherited from another App
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? IsInherited { get; init; }

    /// <summary>
    /// Marks the App which is global for global settings
    /// </summary>
    public bool IsGlobalApp { get; init; }

    /// <summary>
    /// Marks the App which is for the site settings
    /// </summary>
    public bool IsSiteApp { get; init; }

    /// <summary>
    /// Marks the App which is the Content app
    /// </summary>
    public bool IsContentApp { get; init; }


    [JsonIgnore(Condition = WhenWritingNull)]
    public string? Icon { get; init; }
}